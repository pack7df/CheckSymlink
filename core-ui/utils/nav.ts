import { History } from 'history';

//function runScripts(element) {
//    var scripts;

//    // Get the scripts
//    scripts = element.getElementsByTagName("script");

//    // Run them in sequence (remember NodeLists are live)
//    continueLoading();

//    function continueLoading() {
//        var script, newscript;

//        var newScripts = [];

//        // While we have a script to load...
//        while (scripts.length) {
//            // Get it and remove it from the DOM
//            script = scripts[0];
//            script.parentNode.removeChild(script);

//            // Create a replacement for it
//            newscript = document.createElement('script');

//            // External?
//            if (script.src) {
//                // Yes, we'll have to wait until it's loaded before continuing
//                newscript.src = script.src;
//            }
//            else {
//                // No, we can do it right away
//                newscript.text = script.text;
//            }

//            // Start the script
//            newScripts.push(newscript);
//        }

//        // All scripts loaded
//        newscript = undefined;

//        for (var i = 0; i < newScripts.length;i++) {
//            document.documentElement.appendChild(newScripts[i]);
//        }
//    }
//}

export function loadPage(url: string, history: History<any>, reload: boolean = false) {
    //if (reload) {
        window.location.href = url;
    //    return;
    //}
    //var xmlHttp = new XMLHttpRequest();
    //xmlHttp.onreadystatechange = function () {
    //    if (xmlHttp.readyState == 4 && xmlHttp.status == 302) {
    //        var location = xmlHttp.getResponseHeader("Location");
    //        if (typeof window !== 'undefined') {
    //            window.location.href = location;
    //        }
    //    }
    //    if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
    //        history.replace(url);
    //        document.write(xmlHttp.response);
    //        //runScripts(document.documentElement);
    //    }
    //};
    //xmlHttp.open("GET", url, true);
    //xmlHttp.send(null);
}