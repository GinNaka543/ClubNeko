#import <UIKit/UIKit.h>
#import <UniformTypeIdentifiers/UniformTypeIdentifiers.h>
#import <MobileCoreServices/MobileCoreServices.h>

extern "C" {
    
    typedef void (*FilePickerCallback)(const char* path);
    static FilePickerCallback g_filePickerCallback = NULL;
    
    @interface FilePickerDelegate : NSObject<UIDocumentPickerDelegate>
    @end
    
    @implementation FilePickerDelegate
    
    - (void)documentPicker:(UIDocumentPickerViewController *)controller didPickDocumentsAtURLs:(NSArray<NSURL *> *)urls {
        if (urls.count > 0) {
            NSURL *url = urls[0];
            
            // セキュリティスコープのアクセス開始
            BOOL accessing = [url startAccessingSecurityScopedResource];
            
            if (accessing) {
                NSString *path = [url path];
                
                // 一時ディレクトリにコピー
                NSString *tempDir = NSTemporaryDirectory();
                NSString *fileName = [url lastPathComponent];
                NSString *tempPath = [tempDir stringByAppendingPathComponent:fileName];
                
                NSError *error;
                NSFileManager *fileManager = [NSFileManager defaultManager];
                
                // 既存のファイルがあれば削除
                if ([fileManager fileExistsAtPath:tempPath]) {
                    [fileManager removeItemAtPath:tempPath error:nil];
                }
                
                // ファイルをコピー
                if ([fileManager copyItemAtPath:path toPath:tempPath error:&error]) {
                    if (g_filePickerCallback != NULL) {
                        g_filePickerCallback([tempPath UTF8String]);
                    }
                } else {
                    NSLog(@"Failed to copy file: %@", error);
                    if (g_filePickerCallback != NULL) {
                        g_filePickerCallback("");
                    }
                }
                
                // セキュリティスコープのアクセス終了
                [url stopAccessingSecurityScopedResource];
            } else {
                if (g_filePickerCallback != NULL) {
                    g_filePickerCallback("");
                }
            }
        }
    }
    
    - (void)documentPickerWasCancelled:(UIDocumentPickerViewController *)controller {
        if (g_filePickerCallback != NULL) {
            g_filePickerCallback("");
        }
    }
    
    @end
    
    static FilePickerDelegate* g_pickerDelegate = nil;
    
    void _ShowFilePicker(const char* extensions[], int extensionCount, FilePickerCallback callback) {
        g_filePickerCallback = callback;
        
        // メインスレッドで実行
        dispatch_async(dispatch_get_main_queue(), ^{
            UIViewController *rootViewController = [UIApplication sharedApplication].keyWindow.rootViewController;
            
            if (rootViewController == nil) {
                NSArray *windows = [[UIApplication sharedApplication] windows];
                for (UIWindow *window in windows) {
                    if (window.rootViewController != nil) {
                        rootViewController = window.rootViewController;
                        break;
                    }
                }
            }
            
            // サポートするファイルタイプを設定
            NSMutableArray<UTType *> *contentTypes = [NSMutableArray array];
            
            // VRMファイル用のカスタムUTType
            if (@available(iOS 14.0, *)) {
                // VRMファイルタイプを追加
                for (int i = 0; i < extensionCount; i++) {
                    NSString *ext = [NSString stringWithUTF8String:extensions[i]];
                    if ([ext isEqualToString:@"vrm"]) {
                        // VRM用のUTTypeを作成
                        UTType *vrmType = [UTType typeWithFilenameExtension:@"vrm"];
                        if (vrmType != nil) {
                            [contentTypes addObject:vrmType];
                        } else {
                            // フォールバック: 一般的なデータタイプ
                            [contentTypes addObject:UTTypeData];
                        }
                    }
                }
                
                // デフォルトでデータタイプも追加
                if (contentTypes.count == 0) {
                    [contentTypes addObject:UTTypeData];
                }
            }
            
            // ドキュメントピッカーを作成
            UIDocumentPickerViewController *picker = nil;
            
            if (@available(iOS 14.0, *)) {
                picker = [[UIDocumentPickerViewController alloc] initForOpeningContentTypes:contentTypes];
            } else {
                // iOS 13以前の場合
                NSArray *types = @[@"public.data", @"public.item"];
                picker = [[UIDocumentPickerViewController alloc] initWithDocumentTypes:types inMode:UIDocumentPickerModeOpen];
            }
            
            // デリゲートを設定
            if (g_pickerDelegate == nil) {
                g_pickerDelegate = [[FilePickerDelegate alloc] init];
            }
            picker.delegate = g_pickerDelegate;
            picker.allowsMultipleSelection = NO;
            
            // ピッカーを表示
            [rootViewController presentViewController:picker animated:YES completion:nil];
        });
    }
    
    void _ShowVRMPicker(FilePickerCallback callback) {
        const char* extensions[] = { "vrm" };
        _ShowFilePicker(extensions, 1, callback);
    }
    
    // iCloudが利用可能かチェック
    bool _IsICloudAvailable() {
        NSURL *ubiquityURL = [[NSFileManager defaultManager] URLForUbiquityContainerIdentifier:nil];
        return ubiquityURL != nil;
    }
    
    // アプリのDocumentsディレクトリパスを取得
    const char* _GetDocumentsPath() {
        NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
        NSString *documentsDirectory = [paths objectAtIndex:0];
        
        static char path[PATH_MAX];
        strcpy(path, [documentsDirectory UTF8String]);
        return path;
    }
}