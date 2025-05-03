// Getting variables from a global object
var startLocation = window.tripData?.startLocation || "";
var endLocation = window.tripData?.endLocation || "";

// Global variables
let map;
let directionsService;
let directionsRenderer;

// The card initialization function should be available in the global area.
window.initMap = function() {
    map = new google.maps.Map(document.getElementById("map"), {
        zoom: 10,
        center: { lat: 52.13, lng: 21.02 }
    });

    directionsService = new google.maps.DirectionsService();
    directionsRenderer = new google.maps.DirectionsRenderer();
    directionsRenderer.setMap(map);

    if (startLocation && endLocation) {
        calculateAndDisplayRoute(startLocation, endLocation);
    }
}

// Route calculation function
function calculateAndDisplayRoute(start, end) {
    directionsService.route({
        origin: start,
        destination: end,
        travelMode: google.maps.TravelMode.DRIVING
    })
        .then((response) => {
            directionsRenderer.setDirections(response);

            // Getting distance and time
            const route = response.routes[0];
            if (route && route.legs.length > 0) {
                const leg = route.legs[0];
                document.getElementById("distance").textContent = leg.distance.text;
                document.getElementById("duration").textContent = leg.duration.text;
            }
        })
        .catch((e) => {
            console.error("Error getting the route:", e);
            document.getElementById("distance").textContent = "Couldn't calculate";
            document.getElementById("duration").textContent = "Couldn't calculate";
        });
}