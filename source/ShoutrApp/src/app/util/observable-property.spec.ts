import { ObservableProperty } from './observable-property';

describe('ObservableProperty', () => {
  it('should create an instance', () => {
    expect(new ObservableProperty("test")).toBeTruthy();
  });
  describe('Change$', () => {
    it("Should only emit one value", () => {
      const unit = new ObservableProperty(1);
      let counter = 0;
      let latestValue: number | undefined;
      unit.Change$.subscribe(n => {
        counter++;
        latestValue = n;
      });

      unit.Value = 2;

      expect(counter).toBe(1);
      expect(unit.Value).toBe(2);
      expect(latestValue).toBe(2);
    });
    it("Should not emit to late subscribers", () => {
      const unit = new ObservableProperty(1);
      let counter = 0;
      unit.Change$.subscribe(n => { counter++; });

      unit.Value = 2;

      //late subscribers do not get previous values
      let counter2 = 0;
      unit.Change$.subscribe(n => { counter2++; });

      expect(counter).toBe(1);
      expect(counter2).toBe(0);
    });
    it("Should not emit duplicate value", () => {
      const unit = new ObservableProperty(1);
      var counter = 0;
      unit.Change$.subscribe(n => { counter++; });

      unit.Value = 1;

      expect(counter).toBe(0);
    });
  });
  describe('Value$', () => {
    it("Should emit two values", () => {
      const unit = new ObservableProperty(1);
      var counter = 0;
      unit.Value$.subscribe(n => { counter++; });

      unit.Value = 2;

      expect(counter).toBe(2);
      expect(unit.Value).toBe(2);
    });
    it("Should not emit duplicate value", () => {
      const unit = new ObservableProperty(1);
      var counter = 0;
      unit.Value$.subscribe(n => { counter++; });

      unit.Value = 1;

      expect(counter).toBe(1);
    });
  });
  describe('Value', () => {
    it("Should have initial value", () => {
      const unit = new ObservableProperty(100);

      expect(unit.Value).toBe(100);
    });
    it("Should have latest value", () => {
      const unit = new ObservableProperty(100);

      unit.Value = 200;

      expect(unit.Value).toBe(200);
    });
  });
});
