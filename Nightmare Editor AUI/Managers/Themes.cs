namespace Nightmare_Editor_AUI.Managers;

public static class Themes
{
    public class Theme
    {
        public string BGColorUp { get; set; }
        public string BGColorLow { get; set; }
        public string SettingsColor { get; set; }
        public string GridColor { get; set; }
        public string GridColorAlt { get; set; }
        public string GridColorBG { get; set; }
        public string DescColor { get; set; }
        public string TopButtonColor { get; set; }
        public string TopButtonHighlight { get; set; }
        public string TopButtonBorder { get; set; }
        public string TopButtonShadow { get; set; }
        public string ButtonColor { get; set; }
    }
    
    public class MenuTheme
    {
        public string BGColorUp_U { get; set; }
        public string BGColorLow_U { get; set; }
        public string BGHighlight_U { get; set; }
        public string BGHighlightAlt_U { get; set; }
        public string BGColorUp_L { get; set; }
        public string BGColorLow_L { get; set; }
        public string BGHighlight_L { get; set; }
        public string BGColorUp_R { get; set; }
        public string BGColorLow_R { get; set; }
        public string BGHighlight_R { get; set; }
        public string TopBarColor { get; set; }
        public string TextColor { get; set; }
        public string HeartColor { get; set; }
    }

    public static Theme Default = new Theme
    {
        BGColorUp = "#ADD8E6",
        BGColorLow = "#FFA580",
        SettingsColor = "#005ada",
        GridColor = "#f04080",
        GridColorAlt = "#d02060",
        GridColorBG = "#ADD8E6",
        DescColor = "#f04080",
        TopButtonColor = "#FFA580",
        TopButtonHighlight = "#FFFFFF",
        TopButtonBorder = "#ADD8E6",
        TopButtonShadow = "#000000",
        ButtonColor = "#f04080",
    };
    
    public static Theme Topaz = new Theme
    {
        BGColorUp = "#808080",
        BGColorLow = "#808080",
        SettingsColor = "#808080",
        GridColor = "#808080",
        GridColorAlt = "#606060",
        GridColorBG = "#808080",
        DescColor = "#808080",
        TopButtonColor = "#808080",
        TopButtonHighlight = "#A0A0A0",
        TopButtonBorder = "#606060",
        TopButtonShadow = "#606060",
        ButtonColor = "#A0A0A0",
    };
    
    public static Theme Cagaroo = new Theme
    {
        BGColorUp = "#333333",
        BGColorLow = "#262626",
        SettingsColor = "#03045e",
        GridColor = "#015ba0",
        GridColorAlt = "#0077b6",
        GridColorBG = "#fb8500",
        DescColor = "#03045e",
        TopButtonColor = "#fb8500",
        TopButtonHighlight = "#FFFFFF",
        TopButtonBorder = "#ffb702",
        TopButtonShadow = "#606060",
        ButtonColor = "#ffb703",
    };

    public static MenuTheme MenuDefault = new MenuTheme
    {
        BGColorUp_U = "#00001d",
        BGColorLow_U = "#00006e",
        BGHighlight_U = "#002eff",
        BGHighlightAlt_U = "#990000",
        BGColorUp_L = "#00006e",
        BGColorLow_L = "#00001d",
        BGHighlight_L = "#002eff",
        BGColorUp_R = "#00006e",
        BGColorLow_R = "#00001d",
        BGHighlight_R = "#002eff",
        TopBarColor = "#ff740c",
        TextColor = "#fcfc01",
        HeartColor = "#004b9a"
    };
    
    public static MenuTheme MenuTopaz = new MenuTheme
    {
        BGColorUp_U = "#1d1d1d",
        BGColorLow_U = "#333333",
        BGHighlight_U = "#b80f0f",
        BGHighlightAlt_U = "#ae2b62",
        BGColorUp_L = "#333333",
        BGColorLow_L = "#1d1d1d",
        BGHighlight_L = "#b80f0f",
        BGColorUp_R = "#333333",
        BGColorLow_R = "#1d1d1d",
        BGHighlight_R = "#b80f0f",
        TopBarColor = "#ae2b62",
        TextColor = "#4b5fff",
        HeartColor = "#b80f0f"
    };
}