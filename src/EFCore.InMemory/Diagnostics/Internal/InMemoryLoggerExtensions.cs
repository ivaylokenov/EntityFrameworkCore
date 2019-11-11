// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Update;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.InMemory.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class InMemoryLoggerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void TransactionIgnoredWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics)
        {
            var definition = InMemoryResources.LogTransactionsNotSupported(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, warningBehavior);
            }

            var diagnosticSourceEnabled = diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name);
            var simpleLogEnabled = warningBehavior == WarningBehavior.Log
                && diagnostics.SimpleLogger.ShouldLog(definition.EventId, definition.Level);

            if (diagnosticSourceEnabled
                || simpleLogEnabled)
            {
                var eventData = new EventData(
                    definition,
                    (d, _) => ((EventDefinition)d).GenerateMessage());

                if (diagnosticSourceEnabled)
                {
                    diagnostics.DiagnosticSource.Write(definition.EventId.Name, eventData);
                }

                if (simpleLogEnabled)
                {
                    diagnostics.SimpleLogger.Log(eventData);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ChangesSaved(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] IEnumerable<IUpdateEntry> entries,
            int rowsAffected)
        {
            var definition = InMemoryResources.LogSavedChanges(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    rowsAffected);
            }

            var diagnosticSourceEnabled = diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name);
            var simpleLogEnabled = warningBehavior == WarningBehavior.Log
                && diagnostics.SimpleLogger.ShouldLog(definition.EventId, definition.Level);

            if (diagnosticSourceEnabled
                || simpleLogEnabled)
            {
                var eventData = new SaveChangesEventData(
                    definition,
                    ChangesSaved,
                    entries,
                    rowsAffected);

                if (diagnosticSourceEnabled)
                {
                    diagnostics.DiagnosticSource.Write(definition.EventId.Name, eventData);
                }

                if (simpleLogEnabled)
                {
                    diagnostics.SimpleLogger.Log(eventData);
                }
            }
        }

        private static string ChangesSaved(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<int>)definition;
            var p = (SaveChangesEventData)payload;
            return d.GenerateMessage(p.RowsAffected);
        }
    }
}
