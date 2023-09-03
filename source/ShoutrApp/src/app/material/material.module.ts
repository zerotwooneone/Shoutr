import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';

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
  ]
})
export class MaterialModule { }
