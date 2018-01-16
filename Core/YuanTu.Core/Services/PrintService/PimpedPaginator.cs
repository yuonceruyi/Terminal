using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using YuanTu.Core.Log;

namespace YuanTu.Core.Services.PrintService
{
    /// <summary>
    ///     This paginator provides document headers, footers and repeating table headers
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class PimpedPaginator : DocumentPaginator //,IDisposable
    {
        /// <summary>
        ///     Allows drawing headers and footers
        /// </summary>
        /// <param name="context">This is the drawing context that should be used</param>
        /// <param name="bounds">The bounds of the header. You can ignore these at your own peril</param>
        /// <param name="pageNr">The page nr (0-based)</param>
        public delegate void DrawHeaderFooter(DrawingContext context, Rect bounds, int page, int pageCount);

        private Definition definition;
        private DocumentPaginator paginator;

        public PimpedPaginator(FlowDocument document, Definition def, bool needCopy = true)
        {
            // Create a copy of the flow document,
            // so we can modify it without modifying
            // the original.
            initialize(needCopy ? Copy(document) : document, def);
        }

        private void initialize(FlowDocument document, Definition def)
        {
            paginator = ((IDocumentPaginatorSource) document).DocumentPaginator;
            paginator.PageSize = def.ContentSize;
            definition = def;

            // Change page size of the document to
            // the size of the content area
            document.ColumnWidth = double.MaxValue; // Prevent columns
            document.PageWidth = definition.ContentSize.Width;
            document.PageHeight = definition.ContentSize.Height;
            document.PagePadding = new Thickness(0);
        }

        public static FlowDocument Copy(FlowDocument document)
        {
            using (var stream = new MemoryStream())
            {
                var sourceDocument = new TextRange(document.ContentStart, document.ContentEnd);
                sourceDocument.Save(stream, DataFormats.XamlPackage);
                var copy = new FlowDocument();
                var copyDocumentRange = new TextRange(copy.ContentStart, copy.ContentEnd);
                copyDocumentRange.Load(stream, DataFormats.XamlPackage);
                return copy;
            }
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            //if (_pages.ContainsKey(pageNumber))
            //	return _pages[pageNumber];
            // Use default paginator to handle pagination
            var originalPage = paginator.GetPage(pageNumber).Visual;
            Console.WriteLine("--- Page {0} / {1} ---", pageNumber + 1, PageCount);
            Logger.Printer.Info(Thread.CurrentThread.Name + "--- Page " + (pageNumber + 1) + " ---");

            var visual = new ContainerVisual();
            var pageVisual = new ContainerVisual
            {
                Transform = new TranslateTransform(
                    definition.ContentOrigin.X,
                    definition.ContentOrigin.Y
                    )
            };

            pageVisual.Children.Add(originalPage);
            visual.Children.Add(pageVisual);

            // Create headers and footers
            if (definition.Header != null)
            {
                visual.Children.Add(CreateHeaderFooterVisual(definition.Header, definition.HeaderRect, pageNumber,
                    PageCount));
            }
            if (definition.Footer != null)
            {
                visual.Children.Add(CreateHeaderFooterVisual(definition.Footer, definition.FooterRect, pageNumber,
                    PageCount));
            }
            return new DocumentPage(
                visual,
                definition.PageSize,
                new Rect(new Point(), definition.PageSize),
                new Rect(definition.ContentOrigin, definition.ContentSize)
                );
        }

        /// <summary>
        ///     Creates a visual to draw the header/footer
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="bounds"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        private Visual CreateHeaderFooterVisual(DrawHeaderFooter draw, Rect bounds, int pageNumber, int pageCount)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                draw(context, bounds, pageNumber, pageCount);
            }
            return visual;
        }
        
        public class Definition
        {
            public DrawHeaderFooter Header, Footer;

            /// <summary>
            ///     Should table headers automatically repeat?
            /// </summary>
            public bool RepeatTableHeaders { get; set; } = false;

            #region Page sizes

            /// <summary>
            ///     PageSize in DIUs
            /// </summary>
            public Size PageSize { get; set; } = new Size(793.5987, 1122.3987);

            /// <summary>
            ///     Margins
            /// </summary>
            public Thickness Margins { get; set; } = new Thickness(96);

            /// <summary>
            ///     Space reserved for the header in DIUs
            /// </summary>
            public double HeaderHeight { get; set; }

            /// <summary>
            ///     Space reserved for the footer in DIUs
            /// </summary>
            public double FooterHeight { get; set; }

            #endregion Page sizes

            #region Some convenient helper properties

            internal Size ContentSize => new Size(PageSize.Width - (Margins.Left + Margins.Right),
                PageSize.Height - (Margins.Top + Margins.Bottom + HeaderHeight + FooterHeight)
                );

            internal Point ContentOrigin => new Point(
                Margins.Left,
                Margins.Top + HeaderRect.Height
                );

            internal Rect HeaderRect => new Rect(
                Margins.Left, Margins.Top,
                ContentSize.Width, HeaderHeight
                );

            internal Rect FooterRect => new Rect(
                Margins.Left, ContentOrigin.Y + ContentSize.Height,
                ContentSize.Width, FooterHeight
                );

            #endregion Some convenient helper properties
        }

        #region DocumentPaginator members

        public override bool IsPageCountValid => paginator.IsPageCountValid;

        public override int PageCount => paginator.PageCount;

        public override Size PageSize
        {
            get { return paginator.PageSize; }
            set { paginator.PageSize = value; }
        }

        public override IDocumentPaginatorSource Source => paginator.Source;

        #endregion DocumentPaginator members
    }
}