namespace ShareIT
{
    /// <summary>
    /// Marker class for the single shared resource file. Views and controllers resolve
    /// strings through IStringLocalizer&lt;SharedResource&gt; / IViewLocalizer, which maps to
    /// Resources/SharedResource.resx and Resources/SharedResource.ar.resx.
    ///
    /// The English text is the resource key, so a missing Arabic entry falls back to English
    /// rather than throwing. That is why the neutral .resx carries no entries.
    /// </summary>
    public class SharedResource
    {
    }
}
