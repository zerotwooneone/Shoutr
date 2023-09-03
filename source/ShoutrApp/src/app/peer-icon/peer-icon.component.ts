import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'zh-peer-icon',
  templateUrl: './peer-icon.component.html',
  styleUrls: ['./peer-icon.component.scss']
})
export class PeerIconComponent implements OnChanges{
  perSide = 5
  private stdLength = this.perSide * this.perSide * 3;
  icon:string[][] = <string[][]>[];
  @Input()peerId:string = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['peerId']?.currentValue) {
      this.stringToIcon(changes['peerId'].currentValue);
    }
  }

  standardizeLength(input:string){
    let str = input;

    while(str.length < this.stdLength){
      str += input;
    }

    if (str.length > this.stdLength){
      str = str.substring(0, this.stdLength);
    }

    return str;
  }

  charToHex(input:string){
    return input.charCodeAt(0).toString(16);
  }

  stringToIcon(input:string){
    let str = this.standardizeLength(input);
    for(let i = 0; i < this.perSide; i++){
      this.icon[i] = [];
    }
    for(let i = 0; i < this.stdLength; i += 3){
      let box = i/3;
      let pos1 = Math.floor(box/this.perSide);
      let pos2 = box%this.perSide;

      let colorBlock = '#' + this.charToHex(str[i]) + this.charToHex(str[i+1]) + this.charToHex(str[i+2]);
      this.icon[pos1][pos2] = colorBlock;
    }
  }
}
