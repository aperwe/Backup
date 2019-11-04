using System.Collections.Generic;
using System.Linq;
using System;

namespace BackupCore
{

    /// <summary>
    /// Database used by backup.
    /// </summary>
    internal partial class BackupConfiguration
    {
        /// <summary>
        /// Returns all backup locations configured in this database.
        /// </summary>
        public IEnumerable<BackupLocation> GetAllBackupLocations()
        {
            return from x in BackupLocations
                   select new BackupLocation(x.Location, x.Priority);
        }

        /// <summary>
        /// Adds a new backup location. Doesn't add empty or null strings and doesn't add duplicate locations.
        /// </summary>
        /// <param name="location">New location to be added.</param>
        public void AddBackupLocation(string location)
        {
            if (string.IsNullOrEmpty(location)) return;
            if (GetAllBackupLocations().Any(existing => existing.Location.Equals(location, StringComparison.InvariantCultureIgnoreCase))) return;
            BackupLocations.AddBackupLocationsRow(location);
        }

        /// <summary>
        /// Removes a backup location. Doesn't remove empty or null strings and doesn't remove non-existing locations.
        /// </summary>
        /// <param name="location">Existing location to be removed.</param>
        public void RemoveBackupLocation(string location)
        {
            var candidate = BackupLocations.Where(loc => loc.Location.Equals(location, StringComparison.InvariantCultureIgnoreCase)).Single();
            BackupLocations.RemoveBackupLocationsRow(candidate);

        }
        /// <summary>
        /// Sets database parameter to the specified value.
        /// </summary>
        /// <param name="param">Parameter name.</param>
        /// <param name="value">Value of the parameter.</param>
        public void SetParam(string param, string value)
        {
            var existingRows = from paramRow in DatabaseParameters
                               where paramRow.ParameterName.Equals(param, StringComparison.InvariantCultureIgnoreCase)
                               select paramRow;
            switch (existingRows.Count())
            {
                case 0: //New param to be added
                    DatabaseParameters.AddDatabaseParametersRow(param, value);
                    break;
                case 1: //Existing param to be updated
                    var updatedRow = existingRows.First();
                    updatedRow.ParameterValue = value;
                    break;
                default: throw new InvalidOperationException(string.Format("Too many params with '{0}' name.", param));
            }
        }

        /// <summary>
        /// Sets database parameter to the specified value that is strongly typed.
        /// When strongly typed, it then can ge retrieved using Get{type}Param() method; for example GetBoolParam().
        /// </summary>
        /// <param name="param">Parameter name.</param>
        /// <param name="value">Value of the parameter.</param>
        public void SetParam<T>(string param, T value)
        {
            SetParam(param, value.ToString());
        }

        /// <summary>
        /// Gets the value of the specified database parameter.
        /// </summary>
        /// <param name="param">Parameter name.</param>
        /// <returns>Value of the parameter. Null if the param is not defined.</returns>
        public string GetParam(string param)
        {
            var existingRows = from paramRow in DatabaseParameters
                               where paramRow.ParameterName.Equals(param, StringComparison.InvariantCultureIgnoreCase)
                               select paramRow;
            switch (existingRows.Count())
            {
                case 0: //No param
                    return null;
                case 1:
                    return existingRows.First().ParameterValue;
                default: throw new InvalidOperationException(string.Format("Too many params with '{0}' name.", param));
            }
        }

        /// <summary>
        /// Gets the value of the specified database parameter as boolean.
        /// </summary>
        /// <param name="param">Parameter name.</param>
        /// <returns>Either true or false. If the parameter is not present, false is returned. If the value cannot be parsed as boolean, exception will be thrown.</returns>
        public bool GetBoolParam(string param)
        {
            var value = GetParam(param);
            if (string.IsNullOrEmpty(value)) return false;
            return bool.Parse(value);
        }

        /// <summary>
        /// Gets a value of a parameter that is actually an enumeration
        /// </summary>
        /// <typeparam name="T">Must be enum</typeparam>
        /// <returns></returns>
        public T GetEnumType<T>()
        {
            var value = GetParam(typeof(T).Name);
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Records information about a file being backed up.
        /// </summary>
        /// <param name="sourcePath">Path of the source file that has been backed up.</param>
        /// <param name="targetPath">Path, where the source file has been backed up.</param>
        public void AddBackedUpFile(string sourcePath, string targetPath)
        {
            BackedUpFiles.AddBackedUpFilesRow(sourcePath, targetPath);
        }

        /// <summary>
        /// Swaps priorities between two backed up folders.
        /// </summary>
        public void SwapPriorities(BackupLocation previous, BackupLocation next)
        {
            var x = (from m in this.BackupLocations
                    where m.Location == previous.Location
                    where m.Priority == previous.Priority
                    select m).Single();
            var y = (from m in this.BackupLocations
                    where m.Location == next.Location
                    where m.Priority == next.Priority
                    select m).Single();

            var tempPri = x.Priority;
            EnforceConstraints = false;
            x.Priority = y.Priority;
            y.Priority = tempPri;
            EnforceConstraints = true;
        }
    }
}
