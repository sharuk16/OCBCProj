const video = document.getElementById("webcam");
const liveView = document.getElementById("livecam");
const enableActivateButton = document.getElementById("activateButton");
$(document).ready(function () {
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
    var sumList = [];
    function parsing() {
        
        model
            .estimateFaces({
                input: document.querySelector("video"),
            })
            .then(function (predictions) {
                $('#confidence').text((predictions[0].faceInViewConfidence * 100) + "%");
                var xlength = Math.sqrt((predictions[0].boundingBox.topLeft[0] - predictions[0].boundingBox.bottomRight[0]) ** 2)
                var ylength = Math.sqrt((predictions[0].boundingBox.topLeft[1] - predictions[0].boundingBox.bottomRight[1]) ** 2)
                var distList = [];
                var list = predictions[0].scaledMesh;
                for (var i = 0; i < list.length; i++) {
                    for (var a = i + 1; a < list.length; a++) {
                        let cord = list[i];
                        let cord1 = list[a];
                        let x1 = cord[0];
                        let y1 = cord[1];
                        let x2 = cord1[0];
                        let y2 = cord1[1];
                        let dist = Math.sqrt(
                            ((x1 - x2) / xlength) ** 2 + ((y1 - y2) / ylength) ** 2);
                        distList.push(dist);
                    }
                }
                var sum = 0;
                for (var ii = 0; ii < distList.length; ii++) {
                    sum += distList[ii];
                }
                console.log(sum);

                sumList.push(sum);
                if (sumList.length > 2) {
                    let totalsum = 0;
                    for (var index = 0; index < sumList.length; index++) {
                        totalsum += sumList[index];
                    };
                    totalsum = Math.floor(totalsum / 3)
                    const constraints = { video: true };
                    navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
                        stream.getTracks().forEach(function (track) {
                            if (track.readyState == 'live' && track.kind === 'video') {
                                track.stop();
                            }
                        });
                    });
                    redirecting(totalsum);
                }
            });
    }
    function facialRegcognition() {
        setInterval(parsing, 1000);
    }
})