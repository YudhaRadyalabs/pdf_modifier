using Microsoft.AspNetCore.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO.Compression;

namespace pdf_modifier.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PdfModifierController : ControllerBase
    {
        private readonly ILogger<PdfModifierController> _logger;

        public PdfModifierController(ILogger<PdfModifierController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "split")]
        public IActionResult Split(IFormFile file)
        {
            var zipStream = new MemoryStream();
            try
            {
                using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;

                    PdfDocument inputPdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                    for (int pageIndex = 0; pageIndex < inputPdf.PageCount; pageIndex++)
                    {
                        PdfDocument outputPdf = new PdfDocument();
                        outputPdf.Version = inputPdf.Version;
                        outputPdf.Info.Title = $"Page {pageIndex + 1} of {inputPdf.Info.Title}";
                        outputPdf.Info.Creator = inputPdf.Info.Creator;

                        outputPdf.AddPage(inputPdf.Pages[pageIndex]);

                        var zipEntry = zipArchive.CreateEntry($"file.pdf", CompressionLevel.Fastest);
                        using (var entryStream = zipEntry.Open())
                        {
                            outputPdf.Save(entryStream, closeStream: false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            zipStream.Position = 0;
            return File(zipStream, "application/zip", "files.zip");
        }
    }
}
