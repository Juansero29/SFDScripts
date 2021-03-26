using SFDScriptInjector.Extensions;
using SFDScriptInjector.Model.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFDScriptInjector
{
    public class ScriptInjector
    {
        private CancellationToken _cancellationToken;
        private Map _map;
        private Script _script;

        public ScriptInjector(CancellationToken token = default)
        {
            _cancellationToken = token;
        }

        public async Task InjectScriptIntoMap(Script scriptToInject, Map mapToBeInjected)
        {
            _map = mapToBeInjected;
            _script = scriptToInject;
            await LoadMapContentIfNull();
            await InjectScriptIntoMap();
        }

        public async Task SaveInjectedMap()
        {
            if (_map.ContentInBytes == null)
            {
                throw new InvalidOperationException("The injected map content to save was null. Not saving the map.");
            }
            await File.WriteAllBytesAsync(_map.Path, _map.ContentInBytes, _cancellationToken);
        }


        #region Loading Map
        private async Task LoadMapContentIfNull()
        {
            if (_map.ContentInBytes == null) await LoadMapContent();
        }

        private async Task LoadMapContent()
        {
            if (MapToInjectPathIsNullOrEmpty()) throw new InvalidOperationException("Cannot recover map file content from an empty map file path.");

            _map.ContentInBytes = await File.ReadAllBytesAsync(_map.Path, _cancellationToken);

            if (MapToInjectContentIsNullOrEmpty()) throw new InvalidDataException($"No content was found inside the map file read at {_map.Path}");
        }

        private bool MapToInjectPathIsNullOrEmpty()
        {
            return string.IsNullOrEmpty(_map.Path);
        }

        private bool MapToInjectContentIsNullOrEmpty()
        {
            return _map.ContentInBytes == null || _map.ContentInBytes.Length < 0;
        }
        #endregion


        #region Writing New Script Into Map
        private async Task InjectScriptIntoMap()
        {
            if (string.IsNullOrEmpty(_script.ScriptText))
            {
                throw new InvalidOperationException("The script to insert into the map file has a null or empty content.");
            }

            await FindingAndReplacingScriptInsideMapContentInBytes();
        }

        private async Task FindingAndReplacingScriptInsideMapContentInBytes()
        {
            await Task.Run(() =>
            {
                var scriptTagInBytes = Encoding.UTF8.GetBytes("c_scrpt");
                var numberOfCharactersInScriptTag = scriptTagInBytes.Length;

                var nextTagInBytes = Encoding.UTF8.GetBytes("c_lr");

                var indexArrayOfFirstTagInBytes = _map.ContentInBytes.Locate(scriptTagInBytes);
                var indexesArrayNextTagInBytes = _map.ContentInBytes.Locate(nextTagInBytes);

                if (indexArrayOfFirstTagInBytes.Length == 0 || indexesArrayNextTagInBytes.Length == 0) return;

                var indexOfFirstTagInBytes = indexArrayOfFirstTagInBytes[0];
                var indexOfNextTagInBytes = indexesArrayNextTagInBytes[0];
                var endOfTransmissionCharacterCount = 1;
                var indexOfFirstSizeByte = indexOfFirstTagInBytes + numberOfCharactersInScriptTag;
                var indexOfByteBeforeFirstSizeByte = indexOfFirstSizeByte - 1;
                var indexOfLastByteOfScript = indexOfNextTagInBytes - endOfTransmissionCharacterCount - 1;
                var indexOfByteAfterLastByteOfScript = indexOfLastByteOfScript + 1;


                var newScriptTextBytes = Encoding.UTF8.GetBytes(_script.ScriptText);
                var newScriptTextInBase64 = Convert.ToBase64String(newScriptTextBytes);

                var newScriptInBase64SizePrefixedInBytes = newScriptTextInBase64.ToSizePrefixedByteArray();

                var byteArrayUntilByteBeforeFirstSizeByte = new byte[indexOfByteBeforeFirstSizeByte + 1];

                var bytesCountFromByteAfterLastByteOfScriptUntilTheEnd = _map.ContentInBytes.Length - indexOfByteAfterLastByteOfScript;
                var byteArrayFromByteAfterLastByteOfScriptUntilTheEnd = new byte[bytesCountFromByteAfterLastByteOfScriptUntilTheEnd + 1];

                Array.Copy(_map.ContentInBytes, sourceIndex: 0, destinationArray: byteArrayUntilByteBeforeFirstSizeByte, destinationIndex: 0, length: indexOfByteBeforeFirstSizeByte + 1);
                Array.Copy(_map.ContentInBytes, sourceIndex: indexOfByteAfterLastByteOfScript, destinationArray: byteArrayFromByteAfterLastByteOfScriptUntilTheEnd, destinationIndex: 0, length: bytesCountFromByteAfterLastByteOfScriptUntilTheEnd);

                _map.ContentInBytes = ByteExtensions.CombineByteArrays(byteArrayUntilByteBeforeFirstSizeByte, newScriptInBase64SizePrefixedInBytes, byteArrayFromByteAfterLastByteOfScriptUntilTheEnd);
            });
        }
        #endregion
    }
}
