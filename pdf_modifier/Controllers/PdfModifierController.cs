using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

//using PdfSharp.Pdf;
//using PdfSharp.Pdf.IO;
using iText.Kernel.Pdf;
using iText.Layout;

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

                    using (PdfReader reader = new PdfReader(stream))
                    using (PdfDocument pdfDoc = new PdfDocument(reader))
                    {
                        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                        {
                            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i));
                            var splitPageText = pageText.Split("\n");
                            //find page
                            var pages = splitPageText.Where(x => x.Contains("dari")).ToList();
                            int strPage = 0;
                            int endpage = 0;
                            foreach (var page in pages)
                            {
                                var numberPage = page.Split("dari");
                                //start page
                                if (int.TryParse(numberPage[0].Trim(), out int resultStr))
                                {
                                    strPage = i;
                                }
                                //end page
                                if (int.TryParse(numberPage[1].Trim(), out int resultEnd))
                                {
                                    endpage = i + resultEnd - 1;
                                }
                            }
                            if (strPage > 0 && endpage > 0)
                            {
                                string noFaktur = splitPageText[2].Replace("Kode dan Nomor Seri Faktur Pajak :", "");
                                var zipEntry = zipArchive.CreateEntry($"{noFaktur}.pdf");
                                using (var entryStream = zipEntry.Open())
                                {
                                    using (PdfWriter pdfWriter = new PdfWriter(entryStream))
                                    using (PdfDocument splitPdfDoc = new PdfDocument(pdfWriter))
                                    {
                                        // Create a Document object
                                        Document document = new Document(splitPdfDoc);
                                        pdfDoc.CopyPagesTo(strPage, endpage, splitPdfDoc);
                                        document.Close();
                                        i = endpage;
                                    }
                                }
                            }
                        }
                    }
                }

                //using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                //using (var stream = new MemoryStream())
                //{
                //    file.CopyTo(stream);
                //    stream.Position = 0;

                //    PdfDocument inputPdf = PdfReader.Open(stream, PdfDocumentOpenMode.Import);

                //    foreach (var page in inputPdf.Pages)
                //    {
                //        string pageText = PdfTextExtractor.GetTextFromPage(page);
                //    }

                //    for (int pageIndex = 0; pageIndex < inputPdf.PageCount; pageIndex++)
                //    {
                //        PdfDocument outputPdf = new PdfDocument();

                //        outputPdf.Version = inputPdf.Version;
                //        outputPdf.Info.Title = $"Page {pageIndex + 1} of {inputPdf.Info.Title}";
                //        outputPdf.Info.Creator = inputPdf.Info.Creator;

                //        outputPdf.AddPage(inputPdf.Pages[pageIndex]);

                //        var zipEntry = zipArchive.CreateEntry($"file.pdf", CompressionLevel.Fastest);
                //        using (var entryStream = zipEntry.Open())
                //        {
                //            outputPdf.Save(entryStream, closeStream: false);
                //        }
                //    }
                //}
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
