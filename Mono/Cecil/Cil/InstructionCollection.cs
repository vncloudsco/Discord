namespace Mono.Cecil.Cil
{
    using Mono.Collections.Generic;
    using System;

    internal class InstructionCollection : Collection<Instruction>
    {
        internal InstructionCollection()
        {
        }

        internal InstructionCollection(int capacity) : base(capacity)
        {
        }

        protected override void OnAdd(Instruction item, int index)
        {
            if (index != 0)
            {
                Instruction instruction = base.items[index - 1];
                instruction.next = item;
                item.previous = instruction;
            }
        }

        protected override void OnInsert(Instruction item, int index)
        {
            if (base.size != 0)
            {
                Instruction instruction = base.items[index];
                if (instruction == null)
                {
                    Instruction instruction2 = base.items[index - 1];
                    instruction2.next = item;
                    item.previous = instruction2;
                }
                else
                {
                    Instruction previous = instruction.previous;
                    if (previous != null)
                    {
                        previous.next = item;
                        item.previous = previous;
                    }
                    instruction.previous = item;
                    item.next = instruction;
                }
            }
        }

        protected override void OnRemove(Instruction item, int index)
        {
            Instruction previous = item.previous;
            if (previous != null)
            {
                previous.next = item.next;
            }
            Instruction next = item.next;
            if (next != null)
            {
                next.previous = item.previous;
            }
            item.previous = null;
            item.next = null;
        }

        protected override void OnSet(Instruction item, int index)
        {
            Instruction instruction = base.items[index];
            item.previous = instruction.previous;
            item.next = instruction.next;
            instruction.previous = null;
            instruction.next = null;
        }
    }
}

