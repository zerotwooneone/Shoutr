import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';

//note: this is just a convenient place to put all the material module references
//  to avoid cluttering the app module
@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ],
  exports: [
    MatExpansionModule,
  ]
})
export class MaterialModule { }
