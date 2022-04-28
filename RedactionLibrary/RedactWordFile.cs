using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace RedactionLibrary
{
    public class RedactWordFile : IRedactWordFile
    {
        public byte[]? Redact(byte[] filenameToRead)
        {
            // Redact Word file
            if (filenameToRead == null)
            {
                return null;
            }
            byte[] returnVal = default!;
            // see https://docs.microsoft.com/en-us/office/open-xml/how-to-remove-a-document-part-from-a-package
            using (MemoryStream mem = new())
            {
                mem.Write(filenameToRead, 0, (int)filenameToRead.Length);
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(mem, true))
                {
                    // Main Document Part
                    MainDocumentPart mainPart = wordDoc.MainDocumentPart;
                    // All Tables
                    List<Table> tables = mainPart.Document.Descendants<Table>().ToList();
                    // properties of all tables
                    IEnumerable<TableProperties> tableProps = mainPart.Document.Descendants<TableProperties>().Where(
                        tp => tp.TableCaption != null);
                    // Go to each property value
                    foreach (TableProperties tProp in tableProps)
                    {
                        // Check for "Workspace" as the name
                        if (tProp.TableCaption.Val.ToString().Equals("Workspace", StringComparison.Ordinal))
                        {
                            Table table = (Table)tProp.Parent;
                            IEnumerable<TableRow> rows = table.Elements<TableRow>();
                            TableRow firstRow = rows.FirstOrDefault();
                            // Remove caption text "Workspace" and replace it with "Redacted" thus we know that this
                            // table has been 'redacted'
                            tProp.TableCaption.Val = "Redacted";
                            // table.
                            // Delete the first row
                            firstRow.Remove();
                        }
                    }
                    // Remove creator (author), revision
                    wordDoc.PackageProperties.Creator = null;
                    wordDoc.PackageProperties.LastModifiedBy = null;
                }
                returnVal = mem.ToArray();
            }
            return returnVal;
        }


        public byte[]? JoinWithoutQuality(byte[]? ar1, byte[]? ar3, byte[]? ar4, byte[]? ar5, byte[]? ar6, byte[]? ar7)
        {
            // Join the DARs parts 1, 3, 4, 5, 6, 7; 2 is quality
            byte[] myresult = default!;
            //byte[][] bytes = default!;
            byte[][] bytes = new byte[6][];
            int i = 0;
            if(ar1 != null)
            {
                bytes[i] = ar1;
                i++;
            }
            if(ar3 != null)
            {
                bytes[i] = ar3;
                i++;
            }
            if(ar4 != null)
            {
                bytes[i] = ar4;
                i++;
            }
            if(ar5 != null)
            {
                bytes[i] = ar5;
                i++;
            }
            if(ar6 != null)
            {
                bytes[i] = ar6;
                i++;
            }
            if(ar7 != null)
            {
                bytes[i] = ar7;
                i++;
            }
            if (i == 0)
            {
                return null;
            }

            // Otherwise we have at least one part!

            myresult = bytes[0];

            for (int j = 1; j < i; j++)
            {
                myresult = JoinTwoFiles(myresult, bytes[j]);
            }


            //// join ar1 and ar3 into ar1
            //myresult = JoinTwoFiles(ar1, ar3);
            //myresult = JoinTwoFiles(myresult, ar4);
            //myresult = JoinTwoFiles(myresult, ar5);
            //myresult = JoinTwoFiles(myresult, ar6);
            //myresult = JoinTwoFiles(myresult, ar7);
            return myresult;
        }


        public byte[] JoinTwoFiles(byte[] first, byte[] second)
        {
            // In general: join file first and second
            byte[] returnVal = default!;

            var mem1 = new MemoryStream(second);
            var mem2 = new MemoryStream();
            mem2.Write(first, 0, (int)first.Length);
            string altChunkId = "AltChunkId" + DateTime.Now.Ticks.ToString();



            using (WordprocessingDocument myDoc = WordprocessingDocument.Open(mem2, true))
            {
                MainDocumentPart mainPart = myDoc.MainDocumentPart;
                AlternativeFormatImportPart chunk =
                    mainPart.AddAlternativeFormatImportPart(
                    AlternativeFormatImportPartType.WordprocessingML, altChunkId);
                chunk.FeedData(mem1);
                AltChunk altChunk = new AltChunk();
                altChunk.Id = altChunkId;

                OpenXmlElement last = myDoc.MainDocumentPart.Document
                    .Body
                    .Elements()
                    .LastOrDefault(e => e is Paragraph || e is AltChunk);
                last.InsertAfterSelf(new Paragraph(
                    new Run(
                        new Break() { Type = BreakValues.Page })));


                mainPart.Document
                    .Body
                    .InsertAfter(altChunk, mainPart.Document.Body
                    .Elements<Paragraph>().Last());
                mainPart.Document.Save();
            }
            returnVal = mem2.ToArray();
            return returnVal;
        }
    }
}
