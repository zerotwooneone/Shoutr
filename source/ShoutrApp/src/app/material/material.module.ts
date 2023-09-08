import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

//note: this is just a convenient place to put all the material module references
//  to avoid cluttering the app module
@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ],
  exports: [
    MatExpansionModule,
    MatProgressBarModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
  ]
})
export class MaterialModule { }
