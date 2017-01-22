﻿namespace Game
{
    public class EmptySymbol : Symbol
    {
        public static string EMPTY = "EM";

        override public void Initialize(string symbolName, MachineConfig machineConfig)
        {
            var areaSize = machineConfig.NullSymbolSize;
            var displayArea = SlotModel.Instance.SlotConfig.DebugSymbolArea;
            Initialize(symbolName, areaSize, displayArea);
        }

        override public void SetState(SymbolState nextState, bool useOverlap = true)
        {
            if (useOverlap == false && _currentState == nextState) return;

            _currentState = nextState;

            //do nothing
        }
    }
}
