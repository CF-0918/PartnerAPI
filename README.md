#PartnerAPI
An ASP.NET Core (.NET 8) Web API that validates partner submissions, applies discount rules, logs requests/responses, and runs in Docker.

Quick Start
==============
Launch profile: Container (Dockerfile) or Project.
Swagger: https://localhost:<port>/swagger


Test Clients
=============
1.Postman: POST to /api/submittrxmessage with JSON body (see below).
2.index.html (included): a simple HTML/JS page to simulate the partner.
  - If the API port changes, edit the top of index.html: 
    const API_URL = "https://localhost:32769/api/SubmitTrxMessage";
    Always click the button to refresh the timestamp & signature before sending.

** In this project has implemented from Q1-Q5
