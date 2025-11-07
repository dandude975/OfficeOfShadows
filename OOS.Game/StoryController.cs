using System;
using OOS.Shared;

namespace OOS.Game
{
    internal class StoryController
    {
        private ProgressState _state;
        private readonly string _baseDir;

        public StoryController()
        {
            _baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _state = Progress.Load(_baseDir);
        }

        public ProgressState State => _state;

        public bool AtLeast(string checkpoint) =>
            string.Compare(_state.Checkpoint, checkpoint, StringComparison.Ordinal) >= 0;

        public bool Flag(string name) => _state.Flags.Contains(name);

        public void SetFlag(string name)
        {
            if (_state.Flags.Add(name))
                Progress.Save(_baseDir, _state);
        }

        public void SetCheckpoint(string checkpoint)
        {
            if (!AtLeast(checkpoint))
            {
                _state.Checkpoint = checkpoint;
                Progress.Save(_baseDir, _state);
            }
        }

        public void ResetProgress()
        {
            _state = new ProgressState();
            Progress.Save(_baseDir, _state);
        }

        // Stubs (only if you still call these somewhere):
        public string Result(string key) => "";
        public string Choice(string key) => "";
    }
}
