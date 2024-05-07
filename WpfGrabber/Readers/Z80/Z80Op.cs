﻿namespace WpfGrabber.Readers.Z80
{
    public enum Z80Op
    {
        Unknown,
        NoniNOP,
        NOP,
        EX,
        DJNZ,
        JR,
        LD,
        ADD,
        INC,
        DEC,
        ADC,
        SUB,
        SBC,
        OR,
        XOR,
        CP,
        AND,
        RET,
        POP,
        JP,
        OUT,
        IN,
        CALL,
        PUSH,
        BIT,
        RES,
        SET,
        NEG,
        LDI,
        CPI,
        INI,
        OUTI,
        LDD,
        CPD,
        IND,
        OUTD,
        LDIR,
        CPIR,
        INIR,
        OTIR,
        LDDR,
        CPDR,
        INDR,
        OTDR,
        RETI,
        RETN,
        IM,
        RRD,
        RLD,
        RLC,
        RRC,
        RL,
        RR,
        SLA,
        SRA,
        SLL,
        SRL,
        EXX,
        DI,
        EI,
        RST,
        HALT,
        RLCA,
        RRCA,
        RLA,
        RRA,
        DAA,
        CPL,
        SCF,
        CCF,
    }
}
