// NSURLSession backend for sdkHttpPostJson (compiled with -fobjc-arc).
#include "net/SdkHttp.h"
#import <Foundation/Foundation.h>

SdkHttpResponse sdkHttpPostJson(const std::string& url,
                                const std::string& jsonBody,
                                const std::vector<std::pair<std::string, std::string>>& headers,
                                double timeoutSec) {
    SdkHttpResponse out;
    @autoreleasepool {
        NSURL* u = [NSURL URLWithString:[NSString stringWithUTF8String:url.c_str()]];
        if (!u || !u.scheme || !u.host) { out.error = "invalid URL"; return out; }
        NSMutableURLRequest* req = [NSMutableURLRequest requestWithURL:u];
        req.HTTPMethod = @"POST";
        req.timeoutInterval = timeoutSec;
        req.cachePolicy = NSURLRequestReloadIgnoringLocalCacheData;
        [req setValue:@"application/json" forHTTPHeaderField:@"Content-Type"];
        for (const auto& h : headers)
            [req setValue:[NSString stringWithUTF8String:h.second.c_str()]
               forHTTPHeaderField:[NSString stringWithUTF8String:h.first.c_str()]];
        req.HTTPBody = [NSData dataWithBytes:jsonBody.data() length:jsonBody.size()];

        __block NSData* body = nil;
        __block NSHTTPURLResponse* resp = nil;
        __block NSError* error = nil;
        dispatch_semaphore_t sem = dispatch_semaphore_create(0);
        NSURLSessionDataTask* t = [[NSURLSession sharedSession] dataTaskWithRequest:req
            completionHandler:^(NSData* d, NSURLResponse* r, NSError* e) {
                body = d; error = e;
                if ([r isKindOfClass:[NSHTTPURLResponse class]]) resp = (NSHTTPURLResponse*)r;
                dispatch_semaphore_signal(sem);
            }];
        [t resume];
        if (dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW,
                (int64_t)((timeoutSec + 1.0) * NSEC_PER_SEC))) != 0) {
            [t cancel];
            out.timedOut = true;
            out.error = "timed out";
            return out;
        }
        if (resp) out.status = (int)resp.statusCode;
        if (body) out.body.assign((const char*)body.bytes, body.length);
        if (!resp) {
            if (error && error.code == NSURLErrorTimedOut) { out.timedOut = true; out.error = "timed out"; }
            else out.error = error ? std::string(error.localizedDescription.UTF8String ?: "network error")
                                   : "no response";
        }
    }
    return out;
}
