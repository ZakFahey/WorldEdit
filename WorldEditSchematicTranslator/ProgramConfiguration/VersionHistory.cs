namespace WorldEditSchematicTranslator;

internal partial class Program
{
    #region [Summary]

    /// <summary>
    ///     [<u><i>2017.05.10</i></u>] &lt;Terraria v1.3.5&gt;
    ///         v<b><see langword="1.0"/></b> - <b>"Introduced"</b>
    /// </summary>

    #endregion
    private static Version V1_0 = null!;
    #region [Summary]

    /// <summary>
    ///     [<u><i>2020.05.21</i></u>] &lt;Terraria v1.4.0&gt;
    ///         v<b><see langword="2.0"/></b> - <b>"1.4.0 Terraria update; XYWH moved out of zipped part"</b>
    /// </summary>

    #endregion
    private static Version V2_0 = null!;
    #region [Summary]

    /// <summary>
    ///     [<u><i>2022.10.20</i></u>] &lt;Terraria v1.4.4&gt;
    ///         v<b><see langword="3.0"/></b> - <b>"1.4.4 Terraria update; Implemented version control"</b>
    /// </summary>

    #endregion
    private static Version V3_0 = null!;
    private static Dictionary<Version, Version> History = null!;
    private static Version LastVersion = null!;
    private static void InitializeVersionHistory()
    {
        History = new()
        {
            [V1_0 = new(1, 0)] = new(1, 3, 5),
            [V2_0 = new(2, 0)] = new(1, 4, 0),
            [V3_0 = new(3, 0)] = new(1, 4, 4),
        };
        LastVersion = History.Keys.Max()!;
    }
}