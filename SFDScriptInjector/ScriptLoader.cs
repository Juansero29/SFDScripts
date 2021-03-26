using SFDScriptInjector.Model.Base;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SFDScriptInjector
{
    public class ScriptLoader
    {
        private Script _scriptBeingLoaded;
        private CancellationToken _cancellationToken;

        public ScriptLoader(CancellationToken token = default)
        {
            _cancellationToken = token;
        }

        public async Task<Script> LoadScriptAsync(string scriptFilePath, string scriptSurroundingRegionName = "Script To Copy")
        {
            _scriptBeingLoaded = new Script() { FilePath = scriptFilePath, SurroundingRegionName = scriptSurroundingRegionName };

            await LoadScriptTextFromPath();

            await LoadScriptTextInsideSurroundingRegionName();

            return _scriptBeingLoaded;
        }

        private async Task LoadScriptTextInsideSurroundingRegionName()
        {
            await Task.Run(() =>
            {
                Regex regex = new Regex($@"#region {_scriptBeingLoaded.SurroundingRegionName}([\s\S\r]*)#endregion");
                var match = regex.Match(_scriptBeingLoaded.ScriptText);

                if (!match.Success) throw new InvalidDataException($"The script at {_scriptBeingLoaded.FilePath} does not have a region named {_scriptBeingLoaded.SurroundingRegionName}");

                _scriptBeingLoaded.ScriptText = $"#region {_scriptBeingLoaded.SurroundingRegionName}\n" + match.Groups[1].Value.ToString() + "\n#endregion";
            });
        }

        private async Task LoadScriptTextFromPath()
        {
            _scriptBeingLoaded.ScriptText = await File.ReadAllTextAsync(_scriptBeingLoaded.FilePath, _cancellationToken);

            if (string.IsNullOrEmpty(_scriptBeingLoaded.ScriptText))
            {
                throw new InvalidDataException($"The file at {_scriptBeingLoaded.FilePath} has no data inside.");
            }
        }

    }
}
