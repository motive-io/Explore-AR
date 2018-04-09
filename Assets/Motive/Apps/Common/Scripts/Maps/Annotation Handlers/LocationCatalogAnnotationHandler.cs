// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.AR.LocationServices;
using Motive.Core.Models;
using Motive.Unity.Apps;
using Motive.Unity.Maps;

public class LocationCatalogAnnotationHandler : SimpleMapAnnotationHandler 
{
    public string CatalogName;

    Catalog<Location> m_locationCatalog;

    protected override void Awake()
    {
        base.Awake();

        AppManager.Instance.Started += (caller, args) =>
            {
                RefreshCatalog();
            };
    }

    public void RefreshCatalog()
    {
        WebServices.Instance.LoadCatalog<Location>(CatalogName, (catalog) =>
            {
                m_locationCatalog = catalog;
                
                foreach (var location in m_locationCatalog)
                {
                    var ann = new MapAnnotation(location);
                    AddAnnotation(location.Id, ann);
                }
            });
    }
}
