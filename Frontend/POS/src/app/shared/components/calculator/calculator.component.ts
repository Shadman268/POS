import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild
} from '@angular/core';

type Operator = '+' | '-' | '*' | '/';

@Component({
  selector: 'app-calculator',
  templateUrl: './calculator.component.html',
  styleUrls: ['./calculator.component.scss']
})
export class CalculatorComponent implements OnChanges {
  @Input() open = false;
  @Output() closed = new EventEmitter<void>();
  @ViewChild('calcWindow') calcWindow?: ElementRef<HTMLElement>;

  display = '0';
  expression = '';
  memory = 0;
  hasMemory = false;
  copied = false;
  error = false;

  private accumulator: number | null = null;
  private pendingOp: Operator | null = null;
  private freshInput = true;
  private lastOperand: number | null = null;
  private lastOp: Operator | null = null;
  private copyTimer?: ReturnType<typeof setTimeout>;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['open']?.currentValue) {
      setTimeout(() => this.calcWindow?.nativeElement.focus());
    }
  }

  close(): void {
    this.closed.emit();
  }

  inputDigit(digit: string): void {
    if (this.error) {
      this.clearAll();
    }

    if (this.freshInput) {
      this.display = digit;
      this.freshInput = false;
      return;
    }

    if (this.digitCount(this.display) >= 12) {
      return;
    }

    this.display = this.display === '0' ? digit : this.display + digit;
  }

  inputDecimal(): void {
    if (this.error) {
      this.clearAll();
    }

    if (this.freshInput) {
      this.display = '0.';
      this.freshInput = false;
      return;
    }

    if (!this.display.includes('.')) {
      this.display += '.';
    }
  }

  setOperator(op: Operator): void {
    if (this.error) {
      return;
    }

    const current = this.currentValue();

    if (this.pendingOp && this.accumulator !== null && !this.freshInput) {
      const result = this.compute(this.accumulator, this.pendingOp, current);
      if (!isFinite(result)) {
        this.setError();
        return;
      }
      this.accumulator = result;
      this.display = this.format(result);
    } else if (this.accumulator === null) {
      this.accumulator = current;
    }

    this.pendingOp = op;
    this.freshInput = true;
    this.lastOp = null;
    this.lastOperand = null;
    this.expression = `${this.format(this.accumulator as number)} ${this.symbol(op)}`;
  }

  equals(): void {
    if (this.error) {
      return;
    }

    let left: number;
    let op: Operator;
    let right: number;

    if (this.pendingOp && this.accumulator !== null) {
      left = this.accumulator;
      op = this.pendingOp;
      right = this.currentValue();
      this.lastOp = op;
      this.lastOperand = right;
    } else if (this.lastOp && this.lastOperand !== null) {
      left = this.currentValue();
      op = this.lastOp;
      right = this.lastOperand;
    } else {
      return;
    }

    const result = this.compute(left, op, right);
    if (!isFinite(result)) {
      this.setError();
      return;
    }

    this.expression = `${this.format(left)} ${this.symbol(op)} ${this.format(right)} =`;
    this.display = this.format(result);
    this.accumulator = null;
    this.pendingOp = null;
    this.freshInput = true;
  }

  percent(): void {
    if (this.error) {
      return;
    }

    const current = this.currentValue();
    let value: number;

    if (this.pendingOp && this.accumulator !== null && (this.pendingOp === '+' || this.pendingOp === '-')) {
      value = this.round(this.accumulator * (current / 100));
    } else {
      value = this.round(current / 100);
    }

    this.display = this.format(value);
    this.freshInput = true;
  }

  toggleSign(): void {
    if (this.error || this.display === '0' || this.display === '0.') {
      return;
    }

    this.display = this.display.startsWith('-')
      ? this.display.slice(1)
      : `-${this.display}`;
  }

  backspace(): void {
    if (this.error || this.freshInput) {
      return;
    }

    const next = this.display.length <= 1 || (this.display.length === 2 && this.display.startsWith('-'))
      ? '0'
      : this.display.slice(0, -1);

    this.display = next === '-' ? '0' : next;
    if (this.display === '0') {
      this.freshInput = true;
    }
  }

  clearEntry(): void {
    if (this.error) {
      this.clearAll();
      return;
    }

    this.display = '0';
    this.freshInput = true;
  }

  clearAll(): void {
    this.display = '0';
    this.expression = '';
    this.error = false;
    this.accumulator = null;
    this.pendingOp = null;
    this.freshInput = true;
    this.lastOperand = null;
    this.lastOp = null;
  }

  memoryClear(): void {
    this.memory = 0;
    this.hasMemory = false;
  }

  memoryRecall(): void {
    if (!this.hasMemory) {
      return;
    }

    if (this.error) {
      this.clearAll();
    }

    this.display = this.format(this.memory);
    this.freshInput = true;
  }

  memoryAdd(): void {
    if (this.error) {
      return;
    }

    this.memory = this.round(this.memory + this.currentValue());
    this.hasMemory = true;
    this.freshInput = true;
  }

  memorySubtract(): void {
    if (this.error) {
      return;
    }

    this.memory = this.round(this.memory - this.currentValue());
    this.hasMemory = true;
    this.freshInput = true;
  }

  copyResult(): void {
    if (this.error) {
      return;
    }

    const value = this.display;
    const done = () => {
      this.copied = true;
      if (this.copyTimer) {
        clearTimeout(this.copyTimer);
      }
      this.copyTimer = setTimeout(() => {
        this.copied = false;
      }, 1200);
    };

    if (navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(value).then(done).catch(() => this.fallbackCopy(value, done));
      return;
    }

    this.fallbackCopy(value, done);
  }

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    if (!this.open) {
      return;
    }

    if (event.key === 'Escape') {
      event.preventDefault();
      this.close();
      return;
    }

    if (this.isTypingInField(event)) {
      return;
    }

    const key = event.key;

    if (key >= '0' && key <= '9') {
      event.preventDefault();
      this.inputDigit(key);
      return;
    }

    switch (key) {
      case '.':
      case ',':
        event.preventDefault();
        this.inputDecimal();
        break;
      case '+':
        event.preventDefault();
        this.setOperator('+');
        break;
      case '-':
        event.preventDefault();
        this.setOperator('-');
        break;
      case '*':
        event.preventDefault();
        this.setOperator('*');
        break;
      case '/':
        event.preventDefault();
        this.setOperator('/');
        break;
      case '%':
        event.preventDefault();
        this.percent();
        break;
      case 'Enter':
      case '=':
        event.preventDefault();
        this.equals();
        break;
      case 'Backspace':
        event.preventDefault();
        this.backspace();
        break;
      case 'Delete':
        event.preventDefault();
        this.clearEntry();
        break;
      default:
        if (event.key.toLowerCase() === 'c' && !event.ctrlKey && !event.metaKey) {
          event.preventDefault();
          this.clearAll();
        }
        break;
    }
  }

  private currentValue(): number {
    return parseFloat(this.display) || 0;
  }

  private compute(left: number, op: Operator, right: number): number {
    switch (op) {
      case '+':
        return this.round(left + right);
      case '-':
        return this.round(left - right);
      case '*':
        return this.round(left * right);
      case '/':
        return right === 0 ? NaN : this.round(left / right);
    }
  }

  private round(value: number): number {
    return Math.round((value + Number.EPSILON) * 1e10) / 1e10;
  }

  private format(value: number): string {
    if (!isFinite(value)) {
      return 'Error';
    }

    const rounded = this.round(value);
    if (Object.is(rounded, -0)) {
      return '0';
    }

    const abs = Math.abs(rounded);
    if (abs !== 0 && (abs >= 1e12 || abs < 1e-8)) {
      return rounded.toExponential(6).replace(/\.?0+e/, 'e');
    }

    return String(rounded);
  }

  private symbol(op: Operator): string {
    switch (op) {
      case '*':
        return '×';
      case '/':
        return '÷';
      default:
        return op;
    }
  }

  private digitCount(value: string): number {
    return value.replace(/[-.]/g, '').length;
  }

  private setError(): void {
    this.display = 'Error';
    this.expression = '';
    this.error = true;
    this.accumulator = null;
    this.pendingOp = null;
    this.freshInput = true;
    this.lastOperand = null;
    this.lastOp = null;
  }

  private isTypingInField(event: KeyboardEvent): boolean {
    const target = event.target as HTMLElement | null;
    if (!target) {
      return false;
    }

    const tag = target.tagName;
    return tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || target.isContentEditable;
  }

  private fallbackCopy(value: string, done: () => void): void {
    const textarea = document.createElement('textarea');
    textarea.value = value;
    textarea.setAttribute('readonly', '');
    textarea.style.position = 'fixed';
    textarea.style.left = '-9999px';
    document.body.appendChild(textarea);
    textarea.select();
    try {
      document.execCommand('copy');
      done();
    } finally {
      document.body.removeChild(textarea);
    }
  }
}
