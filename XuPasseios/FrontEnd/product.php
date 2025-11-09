<?php
session_start();
$ch = curl_init();
$item = 0;

$temp = explode("/",$_SERVER['REQUEST_URI']);
$productId = (end($temp));

$api_url = "https://localhost:7140/api/products/" . $productId;

curl_setopt($ch, CURLOPT_SSL_VERIFYPEER , false);
curl_setopt($ch, CURLOPT_SSL_VERIFYHOST , false);


curl_setopt($ch, CURLOPT_URL, $api_url);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

$response = curl_exec($ch);

if(curl_errno($ch)){
    echo 'cURL error: '. curl_error($ch);
}

curl_close($ch);

$data = json_decode($response, true);

if(!$data)
{
    echo 'Failed to decode JSON response.';
}
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>E-Commerce</title>
    <link rel="stylesheet" href="/public/styles/product.css">
    <base href="http://localhost:3000"/>
</head>
<body>
    <header class="page__header">
        <div class="header__logo">
            <a href="index.php">
                <figure>
                    <img height="80px" width="80px" src="https://placehold.co/80x80" alt="E-Commerce Logo">
                </figure>
            </a>
        </div>
        <div class="page__header__search">
            <input type="text" class="page__header__text" name="search" id="searchBox">
        </div>
        <div>
            <a href="cart.php">
                <figure>
                    <img heigth="80px" width="80px" src="https://placehold.co/80x80" alt="Cart">
                </figure>
            </a>
        </div>
        <div>
            <a href="register.php">
                <figure>
                    <img heigth="80px" width="80px" src="https://placehold.co/80x80" alt="Register">
                </figure>
            </a>
        </div>
    </header>

    <main class="page__main">
        <div class="page__main__productdetails">
            <?php
                echo '<div class="ProductDetails__Container">
                            <div class="ProductDetails__Image">
                                <figure>
                                    <img height="320px" width="320px" src="https://placehold.co/320x320" alt="Product Figure">
                                </figure>
                            </div>
                            <div class="ProducDetails__Description">
                                <div>
                                    <h2 class="main__product__h2">'. $data['productName'] .'</h2>
                                    <p class="main__product__description">'. $data['productDescription'] .'</p>
                                </div>
                                <div class="ProductDetails__Actions__Caracteristics">
                                    <p>
                                        <ul>
                                            <li>SKU: '. $data['sku'].'</li>
                                            <li>Weight:'. $data['productWeight'].'<br/></li>
                                            <li>A x L x P: <var>'.$data['productHeight'].'</var>&nbsp;x&nbsp;<var>'.$data['productWidth'].'</var>&nbsp;x&nbsp;<var>'.$data['productDepth'].'</var></li>
                                        </ul>
                                    </p>
                                </div>
                                <div>
                                    <a href="index.php"><button class="ProducDetails__Button__Return">Voltar</button></a>
                                </div>
                            </div>
                            <div class="ProductDetails__Actions">
                                    <div class="ProductDetails__Actions__Price">
                                        <p>Preço: R$&nbsp;'.$data['price'].'</p>
                                    </div>
                                    <div class="ProductDetails__Actions__CEP">
                                        <span>CEP:</span>
                                        <input type="text" maxlength="9"></input>
                                    </div>
                                    <form action="cart.php" method="POST">
                                        <input type="hidden" name="productId" value="'. $productId .'"/>
                                        <div class="ProductDetails__Actions__AddCart">
                                            <input type="submit" value="Comprar"></input>
                                        </div>
                                    </form>
                            </div>
                    </div>';
            ?>
        </div>

    </main>
    
    <footer>
         <?php print_r($data); ?>
    </footer>
</body>
</html>
