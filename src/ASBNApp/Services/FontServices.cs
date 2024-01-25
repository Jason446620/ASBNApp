
// service to load fonts -> because the browser only has access to one, we need to load in anything else



using PdfSharp.Fonts;

public class FontServices
{

    // Constructor & DI variables
    public FontServices(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    private readonly HttpClient _httpClient;


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<Fonts> LoadFonts()
    {
        Fonts fonts = new Fonts()
        {
            CourierNew = await GetFontData("CourierNew.ttf")
        };

        return fonts;
    }


    /// <summary>
    /// Load the actual font file via an http request from our wwwroot folder,
    /// return it as a bytestream
    /// </summary>
    /// <param name="name">Name of the font</param>
    /// <returns>Font data as a Bytestream</returns>
    private async Task<byte[]> GetFontData(string name)
    {
        var sourceStream = await _httpClient.GetStreamAsync($"fonts/{name}");
        using MemoryStream memoryStream = new();

        sourceStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

}