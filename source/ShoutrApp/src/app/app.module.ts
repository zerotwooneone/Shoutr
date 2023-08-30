import { NgModule, isDevMode } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ServiceWorkerModule } from '@angular/service-worker';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ConnectedComponent } from './connected/connected.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { ConnectingComponent } from './connecting/connecting.component';
import { BackendModule } from './backend/backend.module';

@NgModule({
  declarations: [
    AppComponent,
    ConnectedComponent,
    PageNotFoundComponent,
    ConnectingComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ServiceWorkerModule.register('ngsw-worker.js', {
      enabled: !isDevMode(),
      // Register the ServiceWorker as soon as the application is stable
      // or after 30 seconds (whichever comes first).
      registrationStrategy: 'registerWhenStable:30000'
    }),
    BrowserAnimationsModule,
    BackendModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
