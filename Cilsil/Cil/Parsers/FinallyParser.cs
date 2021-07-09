// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Cilsil.Extensions;
using Cilsil.Sil;
using Cilsil.Sil.Expressions;
using Cilsil.Sil.Instructions;
using Cilsil.Sil.Types;
using Cilsil.Utils;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Cilsil.Cil.Parsers
{
    /// <summary>
    /// Represents program expressions.
    /// </summary>
    internal class FinallyParser : ExceptionParser
    {
        protected override bool ParseCilInstructionInternal(Instruction instruction,
                                                            ProgramState state)
        {
            return false;
        }
        
        public override bool TranslateBlock(ProgramState state)
        {
            var doorNode = state.PreviousNode;

            CfgNode atriumNode = null;
            Expression byteCodeVariable = null;
            Typ byteCodeVariableType = null;
            // Checks if there is a attrium node for the offset that we can reuse.
            (var attriumNodeAtOffset, var excessiveVisits) =
                state.GetOffsetNode(state.CurrentInstruction.Offset);
            if (attriumNodeAtOffset != null)
            {
                atriumNode = attriumNodeAtOffset;
                return true;
            }
            else
            {
                var catchVariable = new LocalVariable(Identifier.CatchIdentifier(), state.Method);
                RegisterLocalVariable(state, catchVariable, new Tvoid());
                atriumNode = CreateAtriumNode(state, new LvarExpression(catchVariable));

                // Connect attrium node to a new finally block door node as its exceptin sink node.
                var atriumExceptionNode = ExceptionParser.CreateExceptionDoor(state);
                (byteCodeVariable, byteCodeVariableType) = state.Pop();
                atriumNode.ExceptionNodes.Add(atriumExceptionNode);
                atriumExceptionNode.Successors.Add(atriumNode);
                atriumNode.Predecessors.Add(atriumExceptionNode);

            }
            atriumNode.Predecessors.Add(doorNode);

            // translate the content of finally block.
            state.PushInstruction(state.CurrentInstruction.Next, atriumNode);      
            do
            {
                var nextInstruction = state.PopInstruction();
                if (!InstructionParser.ParseCilInstruction(nextInstruction, state, false))
                {
                    Log.RecordUnfinishedMethod(state.Method.GetCompatibleFullName(),
                                               nextInstruction.RemainingInstructionCount());
                    return false;
                }
                if (nextInstruction.OpCode.Code == Code.Endfinally)
                {
                    state.PopInstruction();
                }
            } while (state.HasInstruction);

            CreateReturnExnNode(state, byteCodeVariable, byteCodeVariableType);

            ExceptionParser.SetExceptionNodePredecessors(atriumNode);

            return true;              
        }

        protected override CfgNode CreateAtriumNode(ProgramState state, LvarExpression catchVariable)
        {
            /* In finally block:
            node 26: Preds:29, 35, 41, 43, 45 Succs:27 EXN: 46
            n$27=*&CatchVar77:java.lang.Object*;
            *&$bcvar6:java.lang.Object*=n$27;
            */

            var atriumNode = base.CreateAtriumNode(state, catchVariable);
            var byteCodeVariableType = new Tstruct("System.Object");
            var byteCodeVariable = new LvarExpression(
                                        new LocalVariable(Identifier.ByteCodeIdentifier(),
                                                          state.Method));
            var fieldIdentifier = ((Load)atriumNode.Instructions[0]).Identifier;
            atriumNode.Instructions.Add(new Store(byteCodeVariable,
                                                  new VarExpression(fieldIdentifier),
                                                  byteCodeVariableType,
                                                  state.CurrentLocation));
            state.PushExpr(byteCodeVariable, byteCodeVariableType);

            return atriumNode;
        }

        /// <summary>
        /// Creates return exn node for finally block.
        /// </summary>
        /// <param name="state">Current program state.</param>
        /// <param name="byteCodeVariable">Byte code variable.</param>
        /// <param name="byteCodeVariableType">Byte code variable type.</param>
        /// <returns>The created return exn node.</returns>
        private CfgNode CreateReturnExnNode(ProgramState state, 
                                            Expression byteCodeVariable,
                                            Typ byteCodeVariableType)
        {
            var retType = state.Method.ReturnType.GetElementType();
            var retExnNode = new StatementNode(state.CurrentLocation,
                                               StatementNode.StatementNodeKind.ReturnStmt,
                                               state.ProcDesc);
            Expression returnVariable = new LvarExpression(
                                            new LocalVariable(Identifier.ReturnIdentifier,
                                                              state.Method));
            
            var variableInstruction = state.PushAndLoad(byteCodeVariable, byteCodeVariableType);
            retExnNode.Instructions.Add(variableInstruction);

            (var returnExnValue, _) = state.Pop();
            var retInstr = new Store(returnVariable,
                                     new ExnExpression(returnExnValue),
                                     Typ.FromTypeReference(retType),
                                     state.CurrentLocation);
            retExnNode.Instructions.Add(retInstr);
            retExnNode.Successors = new List<CfgNode> { state.ProcDesc.ExitNode };
            RegisterNode(state, retExnNode);
            
            return retExnNode;
        }
    }
}