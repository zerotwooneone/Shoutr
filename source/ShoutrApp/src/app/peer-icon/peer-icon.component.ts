import { Component, Input, OnChanges, SimpleChanges, numberAttribute } from '@angular/core';

@Component({
  selector: 'zh-peer-icon',
  templateUrl: './peer-icon.component.html',
  styleUrls: ['./peer-icon.component.scss']
})
export class PeerIconComponent implements OnChanges{
  @Input({transform:numberAttribute})blocksPerSide:number = 5;
  private readonly stdLength=() => this.blocksPerSide * this.blocksPerSide * 3;
  icon:string[][] = <string[][]>[];
  @Input()peerId:string = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['peerId']?.currentValue) {
      this.stringToIcon(changes['peerId'].currentValue);
    }
  }

  standardizeLength(input:string){
    let str = input;

    while(str.length < this.stdLength()){
      str += input;
    }

    if (str.length > this.stdLength()){
      str = str.substring(0, this.stdLength());
    }

    return str;
  }

  charToHex(input:string){
    let ascii = input.charCodeAt(0);
    return ((ascii-48) * 25).toString(16);
  }

  hashCode(input:string) {
    var hash = 0,
      i, chr;
    if (input.length === 0) return hash;
    for (i = 0; i < input.length; i++) {
      chr = input.charCodeAt(i);
      hash = ((hash << 5) - hash) + chr;
      hash |= 0; // Convert to 32bit integer
    }
    return Math.abs(hash);
  }

  stringToIcon(input:string){
    let str = this.standardizeLength(this.hashCode(input).toString());
    for(let i = 0; i < this.blocksPerSide; i++){
      this.icon[i] = [];
    }
    for(let i = 0; i < this.stdLength(); i += 3){
      let box = i/3;
      let pos1 = Math.floor(box/this.blocksPerSide);
      let pos2 = box%this.blocksPerSide;

      let colorBlock = '#' + this.charToHex(str[i]) + this.charToHex(str[i+1]) + this.charToHex(str[i+2]);
      this.icon[pos1][pos2] = colorBlock;
    }
  }
}
