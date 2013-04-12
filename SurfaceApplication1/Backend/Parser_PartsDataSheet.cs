using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;

namespace SurfaceApplication1
{
    class Parser_PartsDataSheet
    {
        /// <summary>
        /// 6/25/2012
        /// Parses the HTML document so that all HTML/CSS comments are removed from the text and only
        /// desired content is displayed in a text file.
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="sourceCode">Source code of a particular page</param>

        public Parser_PartsDataSheet(String sourceCode)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sourceCode);
            
            string result = "";

            try
            {
                foreach (HtmlNode comment in doc.DocumentNode.SelectNodes("//comment()|//style"))
                {
                    comment.ParentNode.RemoveChild(comment);
                }

                var nodes = doc.DocumentNode.SelectNodes("//div[@id=\"content\"]");

                foreach (var item in nodes)
                {
                    result += item.InnerText;
                }

                //TextWriter tw = new StreamWriter("testingHTMLAgiPack.txt");
                //TextWriter tw = new StreamWriter("YEAST.txt");
                TextWriter tw = new StreamWriter("SynBio.txt");
                tw.Write(result);
                tw.Close();
            }
            catch
            {
                Console.WriteLine("ERROR!");
            }
        }
    }
}
