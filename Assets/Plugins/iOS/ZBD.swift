import Foundation
import UIKit
import WebKit
import DeviceCheck
import FingerprintPro

// Globals
var webView: WKWebView?
var globalDeviceId: String?


public class ZBDViewController : UIViewController {
    public override func viewDidLoad() {
        super.viewDidLoad()
        self.view.backgroundColor = UIColor.white
    }
}


public class ZBDSwiftNative : UIViewController, WKScriptMessageHandler {
    public static func start(urlString: String) {
        DispatchQueue.main.async {
            if let rootViewController = UIApplication.shared.windows.first?.rootViewController {
                let viewController = ZBDViewController()
                let webConfiguration = WKWebViewConfiguration()
                let contentController = WKUserContentController()
                let scriptMessageHandler = ZBDSwiftNative()
                contentController.add(scriptMessageHandler, name: "iOSInterface")
                webConfiguration.userContentController = contentController
                webView = WKWebView(frame: viewController.view.bounds, configuration: webConfiguration)

                if let url = URL(string: urlString) {
                    let request = URLRequest(url: url)
                    webView?.load(request)
                }

                // Add the WKWebView to the viewController's view
                viewController.view.addSubview(webView!)

                rootViewController.present(viewController, animated: true)
            }
        }
        
        
    }
    

    public static func injectJavascript(_ js: String) {
        webView?.evaluateJavaScript(js, completionHandler: nil)
    }
    
    public func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
         print("Message from JavaScript1: \(message.body)")
        if message.name == "iOSInterface", let messageBody = message.body as? String {
            handleMessageFromWebview(messageBody)
        }
    }
        
    public func handleMessageFromWebview(_ message: String) {
        print("Message from JavaScript: \(message)")
        //closeWebview()
        ZBDSwiftNative.callUnityMethod(message)
    }
    
    public static func closeWebview() {
        DispatchQueue.main.async {
            UIApplication.shared.windows.first?.rootViewController?.dismiss(animated: true)
        }
        
    }
    
    static func callUnityMethod(_ message: String) {
        let uf = UnityFramework.getInstance()
        // Call a method on a specified GameObject.
        uf?.sendMessageToGO(
            withName: "ZBDRewardsController",
            functionName: "OnWebviewMessage",
            message: message)
    }
    
}

// FingerprintPro
public class FingerprintProController {
 

   public static func getRequestId(customUserId: String) async -> String {
        do {
            let client = FingerprintProFactory.getInstance("zOh6gInivwjyHVMmjPyv")
            var metadata = Metadata(linkedId: customUserId)
            metadata.setTag(Bundle.main.bundleIdentifier ?? "unknown", forKey:  "sdk_game_id")
            let visitorId = try await client.getVisitorIdResponse(metadata);
            return visitorId.requestId; 
        }
        catch {
            print("An error occurred: \(error)")
            return ""
        }
    }


}

// --- Public Functions ---

@_cdecl("ZBDSwiftOpenWebview")
public func ZBDSwiftOpenWebview(_ url: UnsafePointer<Int8>) {
    ZBDSwiftNative.start(urlString: String(cString: url))
}

@_cdecl("ZBDSwiftCloseWebview")
public func ZBDSwiftCloseWebview() {
    ZBDSwiftNative.closeWebview()
}

@_cdecl("ZBDSwiftInjectJS")
public func ZBDSwiftInjectJS(_ js: UnsafePointer<Int8>) {
    let jsString = String(cString: js)
    let jsFunction = "receiveMessageFromClient('\(jsString)')"
    print("Injecting JS: \(jsFunction)")
    // Use the global webView variable
    ZBDSwiftNative.injectJavascript(jsFunction)
}

@_cdecl("ZBDSwiftGetRequestId")
public func ZBDSwiftGetRequestId(customUserIdInt:UnsafePointer<Int8>) -> UnsafePointer<Int8> {
    var requestId = ""
    var customUserId = String(cString: customUserIdInt)
    print("customUserId:'\(customUserId)'")
    let semaphore = DispatchSemaphore(value: 0)
    let queue = DispatchQueue(label: "io.zebedee.sdk")
    queue.async {
        Task {
            do {
                requestId = try await FingerprintProController.getRequestId(customUserId: customUserId)
            } catch {
                print("Failed to get visitor ID: \(error)")
            }
            semaphore.signal()
        }
    }
    semaphore.wait()
    return UnsafePointer<Int8>((requestId as NSString).utf8String!)
} 

@_cdecl("showNativeAlert")
public func showNativeAlert(title: UnsafePointer<Int8>, message: UnsafePointer<Int8>) {
    let titleString = String(cString: title)
    let messageString = String(cString: message)

    DispatchQueue.main.async {
        let alertController = UIAlertController(title: titleString, message: messageString, preferredStyle: .alert)
        let okAction = UIAlertAction(title: "OK", style: .default, handler: nil)
        alertController.addAction(okAction)

        if let rootViewController = UIApplication.shared.windows.first?.rootViewController {
            rootViewController.present(alertController, animated: true, completion: nil)
        }
    }
}
