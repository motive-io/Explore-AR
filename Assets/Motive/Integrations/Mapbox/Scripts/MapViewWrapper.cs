// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

#if MOTIVE_MAPBOX
using Mapbox.Map;
using Mapbox.Unity;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
#endif

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Uses a TileDriver to render Mapbox tiles.
    /// </summary>
    public class MapViewWrapper : MonoBehaviour
        
#if MOTIVE_MAPBOX
        , IMap, IMapTileRenderer
#endif
    {
        public MapView MapView;
#if MOTIVE_MAPBOX
        public AbstractMapVisualizer MapVisualizer;
#endif
        public Vector2 TileSize = new Vector2(2.5f, 2.5f);
        public Transform RootTransform;

        public Vector3 XAxis = Vector3.right;
        public Vector3 YAxis = Vector3.up;
        public Vector3 HAxis = -Vector3.forward;

#if MOTIVE_MAPBOX
        class TileInfo
        {
            public UnityTile UnityTile;
            //public GameObject TileObj;
            public UnwrappedTileId TileId;
        }

        TileInfo[,] m_tileInfo;

        int m_tileCountX;
        int m_tileCountY;
        bool m_ready;

        public Vector2d CenterMercator
        {
            get 
            {
                var tileId = new UnwrappedTileId(
                    MapView.TileDriver.TileZoom,
                    (int)MapView.TileDriver.TileCenter.X,
                    (int)MapView.TileDriver.TileCenter.Y);

                return Conversions.TileBounds(tileId).Center;
            }
        }

        public float WorldRelativeScale
        {
            get
            {
                var tileId = new UnwrappedTileId(
                    MapView.TileDriver.TileZoom,
                    (int)MapView.TileDriver.TileCenter.X,
                    (int)MapView.TileDriver.TileCenter.Y);

                var referenceTileRect = Conversions.TileBounds(tileId);

                return (TileSize.x) / (float)referenceTileRect.Size.x;
            }
        }

        public Vector2d CenterLatitudeLongitude
        {
            get 
            {
                return new Vector2d
                {
                    x = MapView.CenterCoordinates.Longitude,
                    y = MapView.CenterCoordinates.Latitude
                };
            }
        }

        public int Zoom
        {
            get 
            {
                return MapView.TileDriver.TileZoom; 
            }
        }

        public Transform Root
        {
            get
            {
                return RootTransform;
            }
        }

        public float UnityTileSize
        {
            get
            {
                return 1f; // ?
            }
        }

        float IMapReadable.Zoom
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public int InitialZoom
        {
            get
            {
                return 0;// throw new System.NotImplementedException();
            }
        }

        public int AbsoluteZoom
        {
            get
            {
                return 0;// throw new System.NotImplementedException();
            }
        }

        public event System.Action OnInitialized = delegate { };

        void Start()
        {
            MapView.TileDriver.RegisterRenderer(this);

            m_tileCountX = MapView.TileDriver.TileCountX; // +BoundaryTiles * 2;
            m_tileCountY = MapView.TileDriver.TileCountY; // +BoundaryTiles * 2;

            m_tileInfo = new TileInfo[m_tileCountX, m_tileCountY];

            for (int x = 0; x < m_tileCountX; x++)
            {
                for (int y = 0; y < m_tileCountY; y++)
                {
                    m_tileInfo[x, y] = new TileInfo();
                }
            }

            MapVisualizer.Initialize(this, MapboxAccess.Instance);
        }

        void PlaceTile(UnityTile tile, int ox, int oy, int x, int y)
        {
            // Sits on a plane which is 10x10
            float sx = TileSize.x;
            float sy = TileSize.y;

            float px = sx * MapView.TileDriver.TileCountX;
            float py = sy * MapView.TileDriver.TileCountY;

            var vx = (x - ox) * sx - (px - sx) / 2;
            var vy = -(y - oy) * sy + (py - sy) / 2;

            //tile.transform.localPosition = new Vector3((x - ox) * sx - (PlaneSize.x - sx) / 2 - BoundaryTiles * sx, 0, -(y - oy) * sy + (PlaneSize.y - sy) / 2 + BoundaryTiles * sy);
            tile.transform.localPosition = vx * XAxis + vy * YAxis;
        }

        void RenderTile(TileInfo info, int ox, int oy, int x, int y, int z)
        {
            /*
            var parameters = new Tile.Parameters
            {
                Fs = m_fileSource,
                Id = new CanonicalTileId(z, (int)x, (int)y)
            };*/

            // Check to see if we need to render a new tile.
            
            if (//info.UnityTile != null &&
                info.UnityTile &&
                info.TileId.Z == z &&
                (int)info.TileId.X == x &&
                (int)info.TileId.Y == y)
            {
                PlaceTile(info.UnityTile, ox, oy, x, y);
                /*
                 * ?? who owns what here?
                info.TileObj.transform.localScale = new Vector3(1 / info.UnityTile.Rect.width * sx, 1 / info.UnityTile.Rect.width * sy, 1 / info.UnityTile.Rect.height * sx);
                info.TileObj.transform.localPosition = new Vector3((x - ox) * sx - (PlaneSize.x - sx) / 2 - BoundaryTiles * sx, 0, (y - oy) * sy - (PlaneSize.y - sy) / 2 - BoundaryTiles * sy);
                 */
            }
            else
            {
                if (info.UnityTile)
                {
                    MapVisualizer.DisposeTile(info.TileId);
                    info.UnityTile = null;
                }

                info.TileId = new Mapbox.Map.UnwrappedTileId(z, x, y);

                //MapVisualizer.LoadTile(info.TileId);
                /*
                info.TileObj = Instantiate(TilePrefab);
                */
                info.UnityTile = MapVisualizer.LoadTile(info.TileId); //info.TileObj.AddComponent<UnityTile>();

                //info.UnityTile.Zoom = z;
                //info.UnityTile.RelativeScale = Conversions.GetTileScaleInMeters(0, z) / Conversions.GetTileScaleInMeters((float)MapController.Instance.MapView.CenterCoordinates.Latitude, z);
                //info.UnityTile.TileCoordinate = new Vector2(x, y);
                //info.UnityTile.Rect = Conversions.TileBounds(info.UnityTile.TileCoordinate, z);

                //info.TileObj.transform.SetParent(this.transform, false);
                //info.TileObj.transform.localScale = new Vector3(1 / info.UnityTile.Rect.width * sx, 1 / info.UnityTile.Rect.width * sy, 1 / info.UnityTile.Rect.height * sx);
                //info.TileObj.transform.localPosition = new Vector3((x - ox) * sx - (PlaneSize.x - sx) / 2 - BoundaryTiles * sx, 0, (y - oy) * sy - (PlaneSize.y - sy) / 2 - BoundaryTiles * sy);

                //info.UnityTile.transform.localScale = new Vector3(1 / info.UnityTile.Rect.width * sx, 1 / info.UnityTile.Rect.width * sy, 1 / info.UnityTile.Rect.height * sx);
                PlaceTile(info.UnityTile, ox, oy, x, y);

                //MapVisualization.ShowTile(info.UnityTile);
            }
        }

        public void SetCenterMercator(Vector2d centerMercator)
        {
            throw new System.NotImplementedException();
        }

        public void SetCenterLatitudeLongitude(Vector2d centerLatitudeLongitude)
        {
            throw new System.NotImplementedException();
        }

        public void SetZoom(int zoom)
        {
            throw new System.NotImplementedException();
        }

        public void CancelAllRequests()
        {
            //throw new System.NotImplementedException();
        }

        public void CancelLoad(TileId tileId)
        {
            //throw new System.NotImplementedException();
        }

        public void LoadTile(TileId tileId, int ox, int oy, System.Action<bool, RendererOperation> onComplete)
        {
            var ix = ((tileId.X - ox) + ox % m_tileCountX) % m_tileCountX;
            var iy = ((tileId.Y - oy) + oy % m_tileCountY) % m_tileCountY;

            var tile = m_tileInfo[ix, iy];

            RenderTile(tile, ox, oy, tileId.X, tileId.Y, tileId.Zoom);
            //throw new System.NotImplementedException();
        }

        public void Refresh()
        {
            //throw new System.NotImplementedException();
        }

        void LateUpdate()
        {
        }

        public void DisposeTile(TileId tileId)
        {
        }


        public void ClearTile(TileId tileId, int ox, int oy)
        {
            var ix = ((tileId.X - ox) + ox % m_tileCountX) % m_tileCountX;
            var iy = ((tileId.Y - oy) + oy % m_tileCountY) % m_tileCountY;

            var tile = m_tileInfo[ix, iy];

            if (tile.UnityTile)
            {
                MapVisualizer.DisposeTile(tile.TileId);
                tile.UnityTile = null;
            }
        }

        public void UpdateLayout()
        {            // The tiles are laid out on plane, this
            // logic positions the plane so that the buildings align with the 
            // map texture.
            var s = MapView.TileDriver.TextureScale;
            var mid = MapView.TileDriver.TileCenter;

            //this.transform.localScale = new Vector3(1 / s.x, 1, 1 / s.y);
            this.RootTransform.localScale =
                XAxis / s.x +
                YAxis / s.y +
                HAxis;
                //new Vector3(1 / s.x, 1, 1 / s.y);

            var countX = MapView.TileDriver.TileCountX;
            var countY = MapView.TileDriver.TileCountY;

            int tileOriginX = (int)(mid.X - (double)countX / 2.0);
            int tileOriginY = (int)(mid.Y - (double)countY / 2.0);

            var midx = tileOriginX + countX / 2;
            var midy = tileOriginY + countY / 2;

            /// ??? Where does this .5 come from?
            var ox = midx - mid.X;// +.5;
            var oy = midy - mid.Y;// +.5;

            float sx = TileSize.x;
            float sy = TileSize.y;

            var vx = (float)ox * sx * RootTransform.localScale.x;
            var vy = (float)-oy * sy * RootTransform.localScale.z;

            //this.transform.localPosition = new Vector3((float)ox * sx * transform.localScale.x, 0, (float)-oy * sy * transform.localScale.z);
            this.RootTransform.localPosition = vx * XAxis + vy * YAxis;

            foreach (var ti in m_tileInfo)
            {
                if (ti.UnityTile)
                {
                    PlaceTile(ti.UnityTile, tileOriginX, tileOriginY, ti.TileId.X, ti.TileId.Y);
                }
            }
        }

        public void SetZoom(float zoom)
        {
            throw new System.NotImplementedException();
        }

        public void SetWorldRelativeScale(float scale)
        {
            throw new System.NotImplementedException();
        }
#endif
    }
}
