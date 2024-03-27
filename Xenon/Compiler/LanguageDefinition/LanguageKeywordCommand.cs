﻿namespace Xenon.Compiler.LanguageDefinition
{
    public enum LanguageKeywordCommand
    {
        INVALIDUNKNOWN,
        SetVar,
        Break,
        Video,
        FilterImage,
        FullImage,
        FitImage,
        AutoFitImage,
        StitchedImage,
        LiturgyImage,
        Liturgy,
        LiturgyVerse,
        TitledLiturgyVerse,
        Reading,
        Sermon,
        AnthemTitle,
        TwoPartTitle,
        TextHymn,
        Verse,
        Copyright,
        ViewServices,
        ViewSeries,
        ApostlesCreed,
        NiceneCreed,
        LordsPrayer,
        Resource,
        Script,
        Script_LiturgyOff,
        Script_OrganIntro,
        Postset, // Doesn't translate into a true AST command, but is of same priority so we'd better add it here
        ScopedVariable,
        VariableScope,
        PostFilter,
        Liturgy2,
        UpNext,
        CustomText,
        Scripted,
        CustomDraw,
        TitledLiturgyVerse2,
        ReStitchedHymn,
        ComplexText,
        DynamicControllerDef,
        HTML,
        HTML2,
    }

}
