import { Injectable } from '@angular/core';
import { ProductView } from 'src/app/core/models/product-data';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  allProducts: ProductView[] = [];

  constructor() { }
}
