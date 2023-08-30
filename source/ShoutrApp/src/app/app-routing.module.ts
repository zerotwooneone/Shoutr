import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ConnectedComponent } from './connected/connected.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { ConnectingComponent } from './connecting/connecting.component';
import { connectedGuard } from './backend/connected.guard';

const routes: Routes = [
  { path: 'connected', component: ConnectedComponent, title: "connected", canActivate: [connectedGuard] },
  { path: '', component: ConnectingComponent, title: "Connecting...", pathMatch: 'full' },
  { path: '**', component: PageNotFoundComponent, title: "Page Not Found" },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
