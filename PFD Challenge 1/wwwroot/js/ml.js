

$(document).ready(function () {
    const video = document.getElementById("webcam");
    const liveView = document.getElementById("livecam");
    const enableActivateButton = document.getElementById("activateButton");
    var model = undefined;
    faceLandmarksDetection
        .load(faceLandmarksDetection.SupportedPackages.mediapipeFacemesh)
        .then(function (loadedModel) {
            model = loadedModel;
        });
    function checkWebCam() {
        return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia);
    }
    if (checkWebCam()) {
        enableActivateButton.addEventListener("click", enableCam);
    } else {
        console.warn("Facial Recognition not supported.");
    }
    function enableCam(event) {
        const constraints = { video: true };
        navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
            video.srcObject = stream;
            video.addEventListener("loadeddata", facialRegcognition);
        });
    }
    function facialRegcognition() {
        
        model
            .estimateFaces({
                input: document.querySelector("video"),
            })
            .then(function (predictions) {
                $('#confidence').text((predictions[0].faceInViewConfidence * 100) + "%");
                
                var sum = 0;
                for (var ii = 0; ii < distList.length; ii++) {
                    sum += distList[ii];
                }
                console.log(sum);
const constraints = { video: true };
                    navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
                        stream.getTracks().forEach(function (track) {
                            if (track.readyState == 'live' && track.kind === 'video') {
                                track.stop();
                            }
                        });
                    });
                    redirecting(totalsum);
            });
    }
})