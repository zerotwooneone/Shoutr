import { PeerModel } from './peer-model';

describe('PeerModel', () => {
  it('should create an instance', () => {
    expect(new PeerModel("some id", "some nickname")).toBeTruthy();
  });
});
