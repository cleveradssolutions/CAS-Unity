//
//  CSAUTypes.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

typedef const void * CASUTypeManagerClientRef;
typedef const void * CASUTypeManagerRef;

typedef const void * CASUTypeViewClientRef;
typedef const void * CASUTypeViewRef;

typedef void (*CASUInitializationCompleteCallback)(CASUTypeManagerClientRef *manager,
                                                   BOOL                     success,
                                                   const char               *error);

#pragma mark - Full Screen ad events
typedef void (*CASUDidLoadedAdCallback)(CASUTypeManagerClientRef *manager);

typedef void (*CASUDidFailedAdCallback)(CASUTypeManagerClientRef *manager,
                                        NSInteger                error);

typedef void (*CASUWillOpeningWithMetaCallback)(CASUTypeManagerClientRef *manager,
                                                NSInteger                net,
                                                double                   cpm,
                                                NSInteger                accuracy);

typedef void (*CASUDidShowAdFailedWithErrorCallback)(CASUTypeManagerClientRef *manager,
                                                     const char               *error);

typedef void (*CASUDidClickedAdCallback)(CASUTypeManagerClientRef *manager);

typedef void (*CASUDidCompletedAdCallback)(CASUTypeManagerClientRef *manager);

typedef void (*CASUDidClosedAdCallback)(CASUTypeManagerClientRef *manager);

typedef void (*CASUATTCompletion)(NSUInteger status);
