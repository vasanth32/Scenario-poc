import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  title: string;
  price: number;
  thumbnail: string;
}

export interface ProductResponse {
  products: Product[];
  total: number;
  skip: number;
  limit: number;
}


@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private http = inject(HttpClient);

  getProducts(): Observable<ProductResponse> {

    return this.http.get<ProductResponse>(
      'https://dummyjson.com/products?limit=0'
    );
  }
}