
# To Develop
- Build and run local dev server
  - `ng serve --open`
    - omit `--open` if you dont want a new browser window each time you run

# One Time Dev Setup
 - install Node.js
   - 18.17.1 at time of writing
 - make sure npm up to date
   - `npm install -g npm`
   - 9.8.1 at time of writing
 - install angular cli
   - `npm install -g @angular/cli`
   - 16.2.0 at time of writing
 
# Notes on how it was created
- new project
  - `ng new ShoutrApp --prefix=zh --routing --skip-git --strict --style=scss --view-encapsulation=ShadowDom`
  - use `--dry-run` to try it out
- add pwa
  - this will allow the site to be installed like an app
  - `ng add @angular/pwa`
- add material
  - this provides a few UI elements. see https://material.angular.io/components/categories
  - `ng add @angular/material`
- add SignalR
  - this is the browser-side of the service which allows the server to call methods in our Typescript classes. Likewise our Typescript classes can call methods on the server. None of these options require a page reload.
  - `npm install @microsoft/signalr`