using System.Web.Mvc;
using System.Xml.Linq;
using MvcSiteMapProvider;
using System.Collections.Generic;
using System.Linq;

public class SitemapController : Controller
{
    public ActionResult Index()
    {
        var siteMap = MvcSiteMapProvider.SiteMaps.Current;
        var rootNode = siteMap.RootNode; // Lấy node gốc
        var nodes = GetAllNodes(rootNode); // Lấy tất cả các nodes

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemap = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(ns + "urlset",
                from node in nodes
                where !string.IsNullOrEmpty(node.Controller) && !string.IsNullOrEmpty(node.Action)
                select new XElement(ns + "url",
                    new XElement(ns + "loc", Url.Action(node.Action, node.Controller, null, Request.Url.Scheme)),
                    new XElement(ns + "changefreq", "weekly"),
                    new XElement(ns + "priority", "0.8")
                )
            )
        );

        return Content(sitemap.ToString(), "text/xml");
    }

    private List<ISiteMapNode> GetAllNodes(ISiteMapNode rootNode)
    {
        var nodes = new List<ISiteMapNode>();

        if (rootNode == null)
        {
            return nodes;
        }

        nodes.Add(rootNode);
        nodes.AddRange(rootNode.Descendants); // Lấy toàn bộ node con

        return nodes;
    }
}
