namespace MistakeProofing.Printing.BarTender
{
    public enum SaveOptionConstants
    {
        PromptSave = 0,
        DoNotSaveChanges = 1,
        SaveChanges = 2
    }

    public enum ColorConstants
    {
        ColorMono = 0,
        Color16 = 1,
        Color256 = 2,
        Color24Bit = 4
    }

    public enum ResolutionConstants
    {
        Screen = 0,
        Printer = 1
    }

    public enum SubStringTypeConstants
    {
        ScreenData = 0,
        Date = 1,
        Time = 2,
        VBScript = 3,
        Database = 4,
        DiskWizard = 5,
        TemplateField = 6,
        LabelObject = 7
    }
}
