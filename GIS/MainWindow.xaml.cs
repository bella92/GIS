using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.ChangeSublayerVisibility
{
    public partial class ChangeSublayerVisibility
    {
        // Create reference to service of US States  
        private string _statesUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2";

        // Create globally available feature table for easy referencing 
        private ServiceFeatureTable _featureTable;

        // Create globally available feature layer for easy referencing 
        private FeatureLayer _featureLayer;

        public ChangeSublayerVisibility()
        {
            InitializeComponent();

            // Setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            //########### TILED LAYER

            // Create uri to the tiled service
            var tiledLayerUri = new Uri(
               "http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer");

            // Create new tiled layer from the url
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tiledLayerUri);

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(tiledLayer);

            //###########




            //########### DINAMYC LAYER

            // Create uri to the map image layer
            var serviceUri = new Uri(
               "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer");

            // Create new image layer from the url
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(serviceUri)
            {
                Name = "World Cities Population"
            };

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(imageLayer);

            //###########




            //########### FEATURE LAYER

            // Create feature table using a url
            _featureTable = new ServiceFeatureTable(new Uri(_statesUrl));

            // Create feature layer using this feature table
            _featureLayer = new FeatureLayer(_featureTable);

            // Set the Opacity of the Feature Layer
            _featureLayer.Opacity = 0.6;

            // Create a new renderer for the States Feature Layer
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(
                SimpleLineSymbolStyle.Solid, Colors.Black, 1);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid, Colors.Yellow, lineSymbol);

            // Set States feature layer renderer
            _featureLayer.Renderer = new SimpleRenderer(fillSymbol);

            // Add feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);


            //###########




            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Wait that the image layer is loaded and sublayer information is fetched
            await imageLayer.LoadAsync();

            // Assign sublayers to the listview
            sublayerListView.ItemsSource = imageLayer.Sublayers;
        }

        private async void OnQueryClicked(object sender, RoutedEventArgs e)
        {
            // Remove any previous feature selections that may have been made 
            _featureLayer.ClearSelection();

            // Begin query process 
            await QueryStateFeature(queryEntry.Text);
        }

        private async Task QueryStateFeature(string stateName)
        {
            try
            {
                // Create a query parameters that will be used to Query the feature table  
                QueryParameters queryParams = new QueryParameters();

                // Construct and assign the where clause that will be used to query the feature table 
                queryParams.WhereClause = "upper(STATE_NAME) LIKE '%" + (stateName.ToUpper()) + "%'";

                // Query the feature table 
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Cast the QueryResult to a List so the results can be interrogated
                var features = queryResult.ToList();

                if (features.Any())
                {
                    // Get the first feature returned in the Query result 
                    Feature feature = features[0];

                    // Add the returned feature to the collection of currently selected features
                    _featureLayer.SelectFeature(feature);

                    // Zoom to the extent of the newly selected feature
                    await MyMapView.SetViewpointGeometryAsync(feature.Geometry.Extent);
                }
                else
                {
                    MessageBox.Show("State Not Found!", "Add a valid state name.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sample error", "An error occurred" + ex.ToString());
            }
        }
    }
}