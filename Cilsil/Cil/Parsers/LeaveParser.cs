// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Cilsil.Utils;
using Mono.Cecil.Cil;

namespace Cilsil.Cil.Parsers
{
    /// A temporary leave instruction parser for finally block support unit test purpose.
    internal class LeaveParser : InstructionParser
    {
        protected override bool ParseCilInstructionInternal(Instruction instruction,
                                                            ProgramState state)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Leave:
                case Code.Leave_S:
                    var previousNode = state.PreviousNode;

                    // Translate finally block nodes for unit test only.
                    ExceptionParser.CreateExceptionDoor(state);
                    FinallyParser finallyParser = new FinallyParser();
                    if (finallyParser.TranslateBlock(state))
                    {
                        // Connect try block nodes to finally block content nodes.
                        state.AppendToPreviousNode = false;
                        state.PushInstruction(instruction.Next, previousNode); 
                        return true;
                    } 
                    return false;             
                default:
                    return false;
            }
        }
    }
}