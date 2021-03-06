﻿namespace Mono.Cecil.Cil
{
    using System;

    internal static class OpCodes
    {
        internal static readonly OpCode[] OneByteOpCode = new OpCode[0xe1];
        internal static readonly OpCode[] TwoBytesOpCode = new OpCode[0x1f];
        public static readonly OpCode Nop = new OpCode(0x50000ff, 0x13000505);
        public static readonly OpCode Break = new OpCode(0x10101ff, 0x13000505);
        public static readonly OpCode Ldarg_0 = new OpCode(0x50202ff, 0x14000501);
        public static readonly OpCode Ldarg_1 = new OpCode(0x50303ff, 0x14000501);
        public static readonly OpCode Ldarg_2 = new OpCode(0x50404ff, 0x14000501);
        public static readonly OpCode Ldarg_3 = new OpCode(0x50505ff, 0x14000501);
        public static readonly OpCode Ldloc_0 = new OpCode(0x50606ff, 0x14000501);
        public static readonly OpCode Ldloc_1 = new OpCode(0x50707ff, 0x14000501);
        public static readonly OpCode Ldloc_2 = new OpCode(0x50808ff, 0x14000501);
        public static readonly OpCode Ldloc_3 = new OpCode(0x50909ff, 0x14000501);
        public static readonly OpCode Stloc_0 = new OpCode(0x50a0aff, 0x13010501);
        public static readonly OpCode Stloc_1 = new OpCode(0x50b0bff, 0x13010501);
        public static readonly OpCode Stloc_2 = new OpCode(0x50c0cff, 0x13010501);
        public static readonly OpCode Stloc_3 = new OpCode(0x50d0dff, 0x13010501);
        public static readonly OpCode Ldarg_S = new OpCode(0x50e0eff, 0x14001301);
        public static readonly OpCode Ldarga_S = new OpCode(0x50f0fff, 0x16001301);
        public static readonly OpCode Starg_S = new OpCode(0x51010ff, 0x13011301);
        public static readonly OpCode Ldloc_S = new OpCode(0x51111ff, 0x14001201);
        public static readonly OpCode Ldloca_S = new OpCode(0x51212ff, 0x16001201);
        public static readonly OpCode Stloc_S = new OpCode(0x51313ff, 0x13011201);
        public static readonly OpCode Ldnull = new OpCode(0x51414ff, 0x1a000505);
        public static readonly OpCode Ldc_I4_M1 = new OpCode(0x51515ff, 0x16000501);
        public static readonly OpCode Ldc_I4_0 = new OpCode(0x51616ff, 0x16000501);
        public static readonly OpCode Ldc_I4_1 = new OpCode(0x51717ff, 0x16000501);
        public static readonly OpCode Ldc_I4_2 = new OpCode(0x51818ff, 0x16000501);
        public static readonly OpCode Ldc_I4_3 = new OpCode(0x51919ff, 0x16000501);
        public static readonly OpCode Ldc_I4_4 = new OpCode(0x51a1aff, 0x16000501);
        public static readonly OpCode Ldc_I4_5 = new OpCode(0x51b1bff, 0x16000501);
        public static readonly OpCode Ldc_I4_6 = new OpCode(0x51c1cff, 0x16000501);
        public static readonly OpCode Ldc_I4_7 = new OpCode(0x51d1dff, 0x16000501);
        public static readonly OpCode Ldc_I4_8 = new OpCode(0x51e1eff, 0x16000501);
        public static readonly OpCode Ldc_I4_S = new OpCode(0x51f1fff, 0x16001001);
        public static readonly OpCode Ldc_I4 = new OpCode(0x52020ff, 0x16000205);
        public static readonly OpCode Ldc_I8 = new OpCode(0x52121ff, 0x17000305);
        public static readonly OpCode Ldc_R4 = new OpCode(0x52222ff, 0x18001105);
        public static readonly OpCode Ldc_R8 = new OpCode(0x52323ff, 0x19000705);
        public static readonly OpCode Dup = new OpCode(0x52425ff, 0x15010505);
        public static readonly OpCode Pop = new OpCode(0x52526ff, 0x13010505);
        public static readonly OpCode Jmp = new OpCode(0x22627ff, 0x13000405);
        public static readonly OpCode Call = new OpCode(0x22728ff, 0x1c1b0405);
        public static readonly OpCode Calli = new OpCode(0x22829ff, 0x1c1b0805);
        public static readonly OpCode Ret = new OpCode(0x7292aff, 0x131b0505);
        public static readonly OpCode Br_S = new OpCode(0x2a2bff, 0x13000f01);
        public static readonly OpCode Brfalse_S = new OpCode(0x32b2cff, 0x13030f01);
        public static readonly OpCode Brtrue_S = new OpCode(0x32c2dff, 0x13030f01);
        public static readonly OpCode Beq_S = new OpCode(0x32d2eff, 0x13020f01);
        public static readonly OpCode Bge_S = new OpCode(0x32e2fff, 0x13020f01);
        public static readonly OpCode Bgt_S = new OpCode(0x32f30ff, 0x13020f01);
        public static readonly OpCode Ble_S = new OpCode(0x33031ff, 0x13020f01);
        public static readonly OpCode Blt_S = new OpCode(0x33132ff, 0x13020f01);
        public static readonly OpCode Bne_Un_S = new OpCode(0x33233ff, 0x13020f01);
        public static readonly OpCode Bge_Un_S = new OpCode(0x33334ff, 0x13020f01);
        public static readonly OpCode Bgt_Un_S = new OpCode(0x33435ff, 0x13020f01);
        public static readonly OpCode Ble_Un_S = new OpCode(0x33536ff, 0x13020f01);
        public static readonly OpCode Blt_Un_S = new OpCode(0x33637ff, 0x13020f01);
        public static readonly OpCode Br = new OpCode(0x3738ff, 0x13000005);
        public static readonly OpCode Brfalse = new OpCode(0x33839ff, 0x13030005);
        public static readonly OpCode Brtrue = new OpCode(0x3393aff, 0x13030005);
        public static readonly OpCode Beq = new OpCode(0x33a3bff, 0x13020001);
        public static readonly OpCode Bge = new OpCode(0x33b3cff, 0x13020001);
        public static readonly OpCode Bgt = new OpCode(0x33c3dff, 0x13020001);
        public static readonly OpCode Ble = new OpCode(0x33d3eff, 0x13020001);
        public static readonly OpCode Blt = new OpCode(0x33e3fff, 0x13020001);
        public static readonly OpCode Bne_Un = new OpCode(0x33f40ff, 0x13020001);
        public static readonly OpCode Bge_Un = new OpCode(0x34041ff, 0x13020001);
        public static readonly OpCode Bgt_Un = new OpCode(0x34142ff, 0x13020001);
        public static readonly OpCode Ble_Un = new OpCode(0x34243ff, 0x13020001);
        public static readonly OpCode Blt_Un = new OpCode(0x34344ff, 0x13020001);
        public static readonly OpCode Switch = new OpCode(0x34445ff, 0x13030a05);
        public static readonly OpCode Ldind_I1 = new OpCode(0x54546ff, 0x16030505);
        public static readonly OpCode Ldind_U1 = new OpCode(0x54647ff, 0x16030505);
        public static readonly OpCode Ldind_I2 = new OpCode(0x54748ff, 0x16030505);
        public static readonly OpCode Ldind_U2 = new OpCode(0x54849ff, 0x16030505);
        public static readonly OpCode Ldind_I4 = new OpCode(0x5494aff, 0x16030505);
        public static readonly OpCode Ldind_U4 = new OpCode(0x54a4bff, 0x16030505);
        public static readonly OpCode Ldind_I8 = new OpCode(0x54b4cff, 0x17030505);
        public static readonly OpCode Ldind_I = new OpCode(0x54c4dff, 0x16030505);
        public static readonly OpCode Ldind_R4 = new OpCode(0x54d4eff, 0x18030505);
        public static readonly OpCode Ldind_R8 = new OpCode(0x54e4fff, 0x19030505);
        public static readonly OpCode Ldind_Ref = new OpCode(0x54f50ff, 0x1a030505);
        public static readonly OpCode Stind_Ref = new OpCode(0x55051ff, 0x13050505);
        public static readonly OpCode Stind_I1 = new OpCode(0x55152ff, 0x13050505);
        public static readonly OpCode Stind_I2 = new OpCode(0x55253ff, 0x13050505);
        public static readonly OpCode Stind_I4 = new OpCode(0x55354ff, 0x13050505);
        public static readonly OpCode Stind_I8 = new OpCode(0x55455ff, 0x13060505);
        public static readonly OpCode Stind_R4 = new OpCode(0x55556ff, 0x13080505);
        public static readonly OpCode Stind_R8 = new OpCode(0x55657ff, 0x13090505);
        public static readonly OpCode Add = new OpCode(0x55758ff, 0x14020505);
        public static readonly OpCode Sub = new OpCode(0x55859ff, 0x14020505);
        public static readonly OpCode Mul = new OpCode(0x5595aff, 0x14020505);
        public static readonly OpCode Div = new OpCode(0x55a5bff, 0x14020505);
        public static readonly OpCode Div_Un = new OpCode(0x55b5cff, 0x14020505);
        public static readonly OpCode Rem = new OpCode(0x55c5dff, 0x14020505);
        public static readonly OpCode Rem_Un = new OpCode(0x55d5eff, 0x14020505);
        public static readonly OpCode And = new OpCode(0x55e5fff, 0x14020505);
        public static readonly OpCode Or = new OpCode(0x55f60ff, 0x14020505);
        public static readonly OpCode Xor = new OpCode(0x56061ff, 0x14020505);
        public static readonly OpCode Shl = new OpCode(0x56162ff, 0x14020505);
        public static readonly OpCode Shr = new OpCode(0x56263ff, 0x14020505);
        public static readonly OpCode Shr_Un = new OpCode(0x56364ff, 0x14020505);
        public static readonly OpCode Neg = new OpCode(0x56465ff, 0x14010505);
        public static readonly OpCode Not = new OpCode(0x56566ff, 0x14010505);
        public static readonly OpCode Conv_I1 = new OpCode(0x56667ff, 0x16010505);
        public static readonly OpCode Conv_I2 = new OpCode(0x56768ff, 0x16010505);
        public static readonly OpCode Conv_I4 = new OpCode(0x56869ff, 0x16010505);
        public static readonly OpCode Conv_I8 = new OpCode(0x5696aff, 0x17010505);
        public static readonly OpCode Conv_R4 = new OpCode(0x56a6bff, 0x18010505);
        public static readonly OpCode Conv_R8 = new OpCode(0x56b6cff, 0x19010505);
        public static readonly OpCode Conv_U4 = new OpCode(0x56c6dff, 0x16010505);
        public static readonly OpCode Conv_U8 = new OpCode(0x56d6eff, 0x17010505);
        public static readonly OpCode Callvirt = new OpCode(0x26e6fff, 0x1c1b0403);
        public static readonly OpCode Cpobj = new OpCode(0x56f70ff, 0x13050c03);
        public static readonly OpCode Ldobj = new OpCode(0x57071ff, 0x14030c03);
        public static readonly OpCode Ldstr = new OpCode(0x57172ff, 0x1a000903);
        public static readonly OpCode Newobj = new OpCode(0x27273ff, 0x1a1b0403);
        public static readonly OpCode Castclass = new OpCode(0x57374ff, 0x1a0a0c03);
        public static readonly OpCode Isinst = new OpCode(0x57475ff, 0x160a0c03);
        public static readonly OpCode Conv_R_Un = new OpCode(0x57576ff, 0x19010505);
        public static readonly OpCode Unbox = new OpCode(0x57679ff, 0x160a0c05);
        public static readonly OpCode Throw = new OpCode(0x8777aff, 0x130a0503);
        public static readonly OpCode Ldfld = new OpCode(0x5787bff, 0x140a0103);
        public static readonly OpCode Ldflda = new OpCode(0x5797cff, 0x160a0103);
        public static readonly OpCode Stfld = new OpCode(0x57a7dff, 0x130b0103);
        public static readonly OpCode Ldsfld = new OpCode(0x57b7eff, 0x14000103);
        public static readonly OpCode Ldsflda = new OpCode(0x57c7fff, 0x16000103);
        public static readonly OpCode Stsfld = new OpCode(0x57d80ff, 0x13010103);
        public static readonly OpCode Stobj = new OpCode(0x57e81ff, 0x13040c03);
        public static readonly OpCode Conv_Ovf_I1_Un = new OpCode(0x57f82ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_I2_Un = new OpCode(0x58083ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_I4_Un = new OpCode(0x58184ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_I8_Un = new OpCode(0x58285ff, 0x17010505);
        public static readonly OpCode Conv_Ovf_U1_Un = new OpCode(0x58386ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_U2_Un = new OpCode(0x58487ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_U4_Un = new OpCode(0x58588ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_U8_Un = new OpCode(0x58689ff, 0x17010505);
        public static readonly OpCode Conv_Ovf_I_Un = new OpCode(0x5878aff, 0x16010505);
        public static readonly OpCode Conv_Ovf_U_Un = new OpCode(0x5888bff, 0x16010505);
        public static readonly OpCode Box = new OpCode(0x5898cff, 0x1a010c05);
        public static readonly OpCode Newarr = new OpCode(0x58a8dff, 0x1a030c03);
        public static readonly OpCode Ldlen = new OpCode(0x58b8eff, 0x160a0503);
        public static readonly OpCode Ldelema = new OpCode(0x58c8fff, 0x160c0c03);
        public static readonly OpCode Ldelem_I1 = new OpCode(0x58d90ff, 0x160c0503);
        public static readonly OpCode Ldelem_U1 = new OpCode(0x58e91ff, 0x160c0503);
        public static readonly OpCode Ldelem_I2 = new OpCode(0x58f92ff, 0x160c0503);
        public static readonly OpCode Ldelem_U2 = new OpCode(0x59093ff, 0x160c0503);
        public static readonly OpCode Ldelem_I4 = new OpCode(0x59194ff, 0x160c0503);
        public static readonly OpCode Ldelem_U4 = new OpCode(0x59295ff, 0x160c0503);
        public static readonly OpCode Ldelem_I8 = new OpCode(0x59396ff, 0x170c0503);
        public static readonly OpCode Ldelem_I = new OpCode(0x59497ff, 0x160c0503);
        public static readonly OpCode Ldelem_R4 = new OpCode(0x59598ff, 0x180c0503);
        public static readonly OpCode Ldelem_R8 = new OpCode(0x59699ff, 0x190c0503);
        public static readonly OpCode Ldelem_Ref = new OpCode(0x5979aff, 0x1a0c0503);
        public static readonly OpCode Stelem_I = new OpCode(0x5989bff, 0x130d0503);
        public static readonly OpCode Stelem_I1 = new OpCode(0x5999cff, 0x130d0503);
        public static readonly OpCode Stelem_I2 = new OpCode(0x59a9dff, 0x130d0503);
        public static readonly OpCode Stelem_I4 = new OpCode(0x59b9eff, 0x130d0503);
        public static readonly OpCode Stelem_I8 = new OpCode(0x59c9fff, 0x130e0503);
        public static readonly OpCode Stelem_R4 = new OpCode(0x59da0ff, 0x130f0503);
        public static readonly OpCode Stelem_R8 = new OpCode(0x59ea1ff, 0x13100503);
        public static readonly OpCode Stelem_Ref = new OpCode(0x59fa2ff, 0x13110503);
        public static readonly OpCode Ldelem_Any = new OpCode(0x5a0a3ff, 0x140c0c03);
        public static readonly OpCode Stelem_Any = new OpCode(0x5a1a4ff, 0x13110c03);
        public static readonly OpCode Unbox_Any = new OpCode(0x5a2a5ff, 0x140a0c03);
        public static readonly OpCode Conv_Ovf_I1 = new OpCode(0x5a3b3ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_U1 = new OpCode(0x5a4b4ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_I2 = new OpCode(0x5a5b5ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_U2 = new OpCode(0x5a6b6ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_I4 = new OpCode(0x5a7b7ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_U4 = new OpCode(0x5a8b8ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_I8 = new OpCode(0x5a9b9ff, 0x17010505);
        public static readonly OpCode Conv_Ovf_U8 = new OpCode(0x5aabaff, 0x17010505);
        public static readonly OpCode Refanyval = new OpCode(0x5abc2ff, 0x16010c05);
        public static readonly OpCode Ckfinite = new OpCode(0x5acc3ff, 0x19010505);
        public static readonly OpCode Mkrefany = new OpCode(0x5adc6ff, 0x14030c05);
        public static readonly OpCode Ldtoken = new OpCode(0x5aed0ff, 0x16000b05);
        public static readonly OpCode Conv_U2 = new OpCode(0x5afd1ff, 0x16010505);
        public static readonly OpCode Conv_U1 = new OpCode(0x5b0d2ff, 0x16010505);
        public static readonly OpCode Conv_I = new OpCode(0x5b1d3ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_I = new OpCode(0x5b2d4ff, 0x16010505);
        public static readonly OpCode Conv_Ovf_U = new OpCode(0x5b3d5ff, 0x16010505);
        public static readonly OpCode Add_Ovf = new OpCode(0x5b4d6ff, 0x14020505);
        public static readonly OpCode Add_Ovf_Un = new OpCode(0x5b5d7ff, 0x14020505);
        public static readonly OpCode Mul_Ovf = new OpCode(0x5b6d8ff, 0x14020505);
        public static readonly OpCode Mul_Ovf_Un = new OpCode(0x5b7d9ff, 0x14020505);
        public static readonly OpCode Sub_Ovf = new OpCode(0x5b8daff, 0x14020505);
        public static readonly OpCode Sub_Ovf_Un = new OpCode(0x5b9dbff, 0x14020505);
        public static readonly OpCode Endfinally = new OpCode(0x7badcff, 0x13000505);
        public static readonly OpCode Leave = new OpCode(0xbbddff, 0x13120005);
        public static readonly OpCode Leave_S = new OpCode(0xbcdeff, 0x13120f01);
        public static readonly OpCode Stind_I = new OpCode(0x5bddfff, 0x13050505);
        public static readonly OpCode Conv_U = new OpCode(0x5bee0ff, 0x16010505);
        public static readonly OpCode Arglist = new OpCode(0x5bf00fe, 0x16000505);
        public static readonly OpCode Ceq = new OpCode(0x5c001fe, 0x16020505);
        public static readonly OpCode Cgt = new OpCode(0x5c102fe, 0x16020505);
        public static readonly OpCode Cgt_Un = new OpCode(0x5c203fe, 0x16020505);
        public static readonly OpCode Clt = new OpCode(0x5c304fe, 0x16020505);
        public static readonly OpCode Clt_Un = new OpCode(0x5c405fe, 0x16020505);
        public static readonly OpCode Ldftn = new OpCode(0x5c506fe, 0x16000405);
        public static readonly OpCode Ldvirtftn = new OpCode(0x5c607fe, 0x160a0405);
        public static readonly OpCode Ldarg = new OpCode(0x5c709fe, 0x14000e05);
        public static readonly OpCode Ldarga = new OpCode(0x5c80afe, 0x16000e05);
        public static readonly OpCode Starg = new OpCode(0x5c90bfe, 0x13010e05);
        public static readonly OpCode Ldloc = new OpCode(0x5ca0cfe, 0x14000d05);
        public static readonly OpCode Ldloca = new OpCode(0x5cb0dfe, 0x16000d05);
        public static readonly OpCode Stloc = new OpCode(0x5cc0efe, 0x13010d05);
        public static readonly OpCode Localloc = new OpCode(0x5cd0ffe, 0x16030505);
        public static readonly OpCode Endfilter = new OpCode(0x7ce11fe, 0x13030505);
        public static readonly OpCode Unaligned = new OpCode(0x4cf12fe, 0x13001004);
        public static readonly OpCode Volatile = new OpCode(0x4d013fe, 0x13000504);
        public static readonly OpCode Tail = new OpCode(0x4d114fe, 0x13000504);
        public static readonly OpCode Initobj = new OpCode(0x5d215fe, 0x13030c03);
        public static readonly OpCode Constrained = new OpCode(0x5d316fe, 0x13000c04);
        public static readonly OpCode Cpblk = new OpCode(0x5d417fe, 0x13070505);
        public static readonly OpCode Initblk = new OpCode(0x5d518fe, 0x13070505);
        public static readonly OpCode No = new OpCode(0x5d619fe, 0x13001004);
        public static readonly OpCode Rethrow = new OpCode(0x8d71afe, 0x13000503);
        public static readonly OpCode Sizeof = new OpCode(0x5d81cfe, 0x16000c05);
        public static readonly OpCode Refanytype = new OpCode(0x5d91dfe, 0x16010505);
        public static readonly OpCode Readonly = new OpCode(0x5da1efe, 0x13000504);
    }
}

