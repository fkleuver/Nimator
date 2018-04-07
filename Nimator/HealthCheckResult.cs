using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the result of running a <see cref="T:Nimator.IHealthCheck" />.
    /// </summary>
    public sealed class HealthCheckResult : IEquatable<HealthCheckResult>
    {
        /// <summary>
        /// The identity of the <see cref="IHealthCheck"/> that brought forth this result.
        /// </summary>
        public Identity CheckId { get; }

        /// <summary>
        /// The severity of the result.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// The status of the checked service.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Additional details about the result.
        /// </summary>
        public IDictionary<string, object> Details { get; }

        /// <summary>
        /// The reason for this result's <see cref="Level"/> and <see cref="Status"/>.
        /// </summary>
        public string Reason => Details.TryGetValue(Constants.Reason, out var reason) ? reason.ToString() : "no details provided";

        /// <summary>
        /// Returns the exception that is set in the details, or null if none is set.
        /// </summary>
        [CanBeNull]
        public Exception Exception => (Details.TryGetValue(Constants.Exception, out var exception) ? exception : null) as Exception;

        // Gets this result's reason including any "Some.Nested.Reason" that were set by Finalize()
        public IEnumerable<KeyValuePair<string, object>> AllReasons => Details.Keys
            .Where(k => Regex.IsMatch(k, @"([a-zA-Z0-9_]+\.)*Reason"))
            .Select(k => new KeyValuePair<string, object>(k, Details[k]));
        
        public ICollection<HealthCheckResult> InnerResults { get; }

        /// <summary>
        /// Returns a flat list containing this <see cref="HealthCheckResult"/> and all of its <see cref="InnerResults"/>
        /// recursively.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<HealthCheckResult> AllResults => this.Flatten(r => r.InnerResults);

        public HealthCheckResult(
            [NotNull]Identity checkId,
            LogLevel level = LogLevel.Info,
            Status status = Status.Unknown)
        {
            Guard.AgainstNull(nameof(checkId), checkId);

            CheckId = checkId;
            Level = level;
            Status = status;
            Details = new Dictionary<string, object>();
            InnerResults = new List<HealthCheckResult>();
        }
        
        public static HealthCheckResult Create([NotNull]Identity checkId)
        {
            return new HealthCheckResult(checkId);
        }

        public static HealthCheckResult Create([NotNull]string checkName)
        {
            Guard.AgainstNull(nameof(checkName), checkName);
            return Create(new Identity(checkName));
        }


        public HealthCheckResult SetStatus(Status status)
        {
            Status = status;
            return this;
        }

        public HealthCheckResult SetLevel(LogLevel level)
        {
            Level = level;
            return this;
        }

        public HealthCheckResult SetReason([CanBeNull]string reason)
        {
            return AddDetail(Constants.Reason, reason);
        }

        public HealthCheckResult SetErrorMessage([CanBeNull]string message)
        {
            return AddDetail(Constants.ErrorMessage, message);
        }

        public HealthCheckResult SetException([CanBeNull]Exception ex)
        {
            return AddDetail(Constants.Exception, ex);
        }

        public HealthCheckResult AddDetail([NotNull]string name, [CanBeNull]object obj)
        {
            Guard.AgainstNullAndEmpty(nameof(name), name);

            if (Details.ContainsKey(name))
            {
                if (Details.Values.Any(v => v == obj))
                {
                    return this;
                }
                var newName = name;
                var counter = 1;
                while (Details.ContainsKey(newName))
                {
                    newName = $"{name}#{counter}";
                    counter++;
                }
                Details.Add(newName, obj);
            }
            else
            {
                Details.Add(name, obj);
            }
            return this;
        }
        
        public HealthCheckResult AddInnerResult([NotNull]HealthCheckResult inner)
        {
            Guard.AgainstNull(nameof(inner), inner);

            InnerResults.Add(inner);
            return this;
        }

        public HealthCheckResult AddInnerResult([NotNull]Action<HealthCheckResult> configure)
        {
            Guard.AgainstNull(nameof(configure), configure);

            return AddInnerResult(CheckId, configure);
        }

        public HealthCheckResult AddInnerResult([NotNull]Identity checkId, [NotNull]Action<HealthCheckResult> configure)
        {
            Guard.AgainstNull(nameof(checkId), checkId);
            Guard.AgainstNull(nameof(configure), configure);

            var inner = new HealthCheckResult(checkId);
            configure(inner);
            return AddInnerResult(inner);
        }

        /// <summary>
        /// Recursively goes through the children (bottom-up) and bubbles up the highest severity+status and pushes reasons
        /// up for results matching the predicate.
        /// </summary>
        public void Finalize([NotNull]Identity checkId, [NotNull]Predicate<HealthCheckResult> pushReasonIfMatch)
        {
            Guard.AgainstNull(nameof(checkId), checkId);
            Guard.AgainstNull(nameof(pushReasonIfMatch), pushReasonIfMatch);

            foreach (var inner in InnerResults)
            {
                inner.Finalize(checkId, pushReasonIfMatch);
                if (inner.Status > Status)
                {
                    Status = inner.Status;
                }
                if (inner.Level > Level)
                {
                    Level = inner.Level;
                }

                if (pushReasonIfMatch(inner))
                {
                    foreach (var kvp in inner.AllReasons)
                    {
                        var newKey = string.Join(".", new[] {checkId.Name, inner.CheckId.Name}.Concat(kvp.Key.Split('.')).Distinct());
                        AddDetail(newKey, kvp.Value);
                    }
                }
            }
        }



        #region IEquatable implementation
        public bool Equals(HealthCheckResult other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(CheckId, other.CheckId) &&
                   Level == other.Level &&
                   Status == other.Status;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HealthCheckResult)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CheckId != null ? CheckId.GetHashCode() : 0);
                // ReSharper disable NonReadonlyMemberInGetHashCode | We're making sure these are not modified after they're initially set
                hashCode = (hashCode * 397) ^ (int)Level;
                hashCode = (hashCode * 397) ^ (int)Status;
                // ReSharper enable NonReadonlyMemberInGetHashCode
                return hashCode;
            }
        }

        public static bool operator ==(HealthCheckResult left, HealthCheckResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HealthCheckResult left, HealthCheckResult right)
        {
            return !Equals(left, right);
        }

        #endregion
    }


    public enum Status
    {
        Okay = 0,
        Unknown = 1,
        Maintenance = 2,
        Warning = 3,
        Critical = 4
    }
}
