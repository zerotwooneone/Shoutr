import { Hub } from './hub';

describe('Hub', () => {
  it('should create an instance', () => {
    expect(new Hub("hub name")).toBeTruthy();
  });
});
