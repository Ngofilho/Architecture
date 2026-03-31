<?php
session_start();
$ch = curl_init();
$item = 0;

$api_url = "https://localhost:7000/api/products";

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
    <link rel="stylesheet" href=".\public\styles\index.css">
    <base href="http://localhost:3000"/>
</head>
<body>
    <?php require_once 'header.php'; ?>

    <main class="page__main">
        <div class="page__main__product">
            <?php
            foreach($data as $value)
            {
                echo '<div id="ProductCard">
                        <a id="ProductCardLink" href="product.php/'. $value['productId'] .'">
                            <figure>
                                <img height="80px" width="80px" src="https://placehold.co/80x80" alt="Product Figure">
                            </figure>
                            <h2 class="main__product__h2">'. $value['productName'] .'</h2>
                        </a>
                    </div>';
            }
            ?>
        </div>

    </main>
    
    <?php require_once 'footer.php'; ?>
</body>
</html>
