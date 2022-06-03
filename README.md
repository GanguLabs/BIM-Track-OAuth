# BIM-Track-OAuth

The project has the necessary code to authenticate a BIM Track user using OAuth2.0 and get a access-token from the user and send an API Call on this behalf.

The boilerplate code for Oauth2.0 is referenced from [googlesamples/oauth-apps-for-windows](https://github.com/googlesamples/oauth-apps-for-windows/tree/master/OAuthDesktopApp)

The [following article][BIM Track OAuth] by BIM Track provides basic details of the steps you need to follow to get started with BIM Track Authentication.

> Note that the "Client_ID" & "Secret" of your app will be provided by BIM Track once you reach out to them 

Changes made after boilerplate Code:

1. Update the "Client_ID" & "Client_Secret" to match your app.
2. The "authorizationEndpoint" & "tokenEndpoint" for BIM Track are provided in the BIM Track [OAuth article][BIM Track OAuth]
3. The "redirectURI" for my BIM Track app was `http://127.0.0.1`. But `http.Prefixes.Add()` function doesn't allow uri's that does NOT end with "/". So I had to remove the "/" at the end for BIMTrack `authorizationRequest` and add it back for  `http.Prefixes.Add()`.
4. Added `getHubs_HttpWebRequest()` API Call to fetch all the list of HUBS that user has access to using the access_token that's fetched with OAuth.
5. Added and commented `getHubs_RestClient()`, that uses [RestSharp](https://restsharp.dev/) to send API Call. RestSharp is supported by PostMan while converting API calls to [code snippets](https://learning.postman.com/docs/sending-requests/generate-code-snippets/)


[BIM Track OAuth]: https://help.bimtrack.co/hc/en-ca/articles/360060182352-Using-OAuth-2-0
