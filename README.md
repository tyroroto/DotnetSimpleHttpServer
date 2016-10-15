# DotnetSimpleHttpServer
Simple http server with Response handle class for GET/POST requset.

Clone and run ConsoleApplication proj
http://localhost:9999 to see result.
http://localhost:9999/getTest
or
post request to http://localhost:9999/postTest

GET/POST reqest can create by UrlRequest class

note 
-if url is empty will redirect to index.html file in folder that set in rootDirectory if file exist.
-after get request if not found any response handle (UrlRequest) will go to find file in rootDirectory if name match
example http://localhost:9999/toto.html -> not found response handle -> rootDirectory/toto.html
